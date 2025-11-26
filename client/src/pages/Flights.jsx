import { useState, useEffect } from 'react'
import axios from 'axios'
import { useNavigate } from 'react-router-dom'
import SeatSelector from '../components/SeatSelector'

function Flights({ user }) {
  const [flights, setFlights] = useState([])
  const [loading, setLoading] = useState(true)
  const [searchParams, setSearchParams] = useState({
    departureCity: '',
    arrivalCity: '',
    departureDate: ''
  })
  const [selectedFlight, setSelectedFlight] = useState(null)
  const [passengers, setPassengers] = useState([{
    firstName: '',
    lastName: '',
    identityNumber: '',
    dateOfBirth: '',
    gender: 'Erkek',
    seatNumber: '',
    seatType: ''
  }])
  const [guestEmail, setGuestEmail] = useState('')
  const [selectedSeats, setSelectedSeats] = useState([])
  const [showSeatSelector, setShowSeatSelector] = useState(false)
  const navigate = useNavigate()

  useEffect(() => {
    fetchFlights()
  }, [])

  const fetchFlights = async (params = {}) => {
    try {
      // Boş parametreleri filtrele
      const filteredParams = {}
      if (params.departureCity) filteredParams.departureCity = params.departureCity
      if (params.arrivalCity) filteredParams.arrivalCity = params.arrivalCity
      if (params.departureDate) filteredParams.departureDate = params.departureDate
      
      const response = await axios.get('/api/flights', { params: filteredParams })
      setFlights(response.data)
    } catch (error) {
      console.error('Uçuşlar yüklenemedi:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleSearch = (e) => {
    e.preventDefault()
    setLoading(true)
    fetchFlights(searchParams)
  }

  const handlePassengerChange = (index, field, value) => {
    const newPassengers = [...passengers]
    newPassengers[index][field] = value
    setPassengers(newPassengers)
  }

  const addPassenger = () => {
    // Yeni yolcu eklendiğinde koltuk seçimlerini sıfırla
    if (selectedSeats.length > 0) {
      const confirmAdd = confirm(
        '⚠️ Yeni yolcu eklendiğinde koltuk seçimleri sıfırlanacak.\n\n' +
        'Devam etmek istiyor musunuz?'
      )
      if (!confirmAdd) return
      
      setSelectedSeats([])
      setShowSeatSelector(false)
    }
    
    setPassengers([...passengers, {
      firstName: '',
      lastName: '',
      identityNumber: '',
      dateOfBirth: '',
      gender: 'Erkek',
      seatNumber: '',
      seatType: ''
    }])
  }

  const handleSeatsSelected = (seats) => {
    setSelectedSeats(seats)
    // Koltukları yolculara ata
    const updatedPassengers = passengers.map((passenger, index) => ({
      ...passenger,
      seatNumber: seats[index]?.seatNumber || '',
      seatType: seats[index]?.seatType || ''
    }))
    setPassengers(updatedPassengers)
  }

  const removePassenger = (index) => {
    // Yolcu kaldırıldığında koltuk seçimlerini sıfırla
    if (selectedSeats.length > 0) {
      const confirmRemove = confirm(
        '⚠️ Yolcu kaldırıldığında koltuk seçimleri sıfırlanacak.\n\n' +
        'Devam etmek istiyor musunuz?'
      )
      if (!confirmRemove) return
      
      setSelectedSeats([])
      setShowSeatSelector(false)
    }
    
    setPassengers(passengers.filter((_, i) => i !== index))
  }

  const handleBooking = async (e) => {
    e.preventDefault()
    
    if (!user && !guestEmail) {
      alert('⚠️ Lütfen giriş yapın veya misafir e-posta adresi girin')
      return
    }

    // Rezervasyon onayı
    const confirmBooking = confirm(
      '🎫 Rezervasyonu onaylıyor musunuz?\n\n' +
      `Yolcu Sayısı: ${passengers.length}\n` +
      `Toplam Tutar: ₺${selectedFlight.price * passengers.length}\n` +
      `Seçilen Koltuklar: ${selectedSeats.length > 0 ? selectedSeats.map(s => s.seatNumber).join(', ') : 'Seçilmedi'}`
    )
    
    if (!confirmBooking) return

    try {
      const token = localStorage.getItem('token')
      let bookingToken = token

      if (!user && guestEmail) {
        const guestResponse = await axios.post('/api/auth/guest', JSON.stringify(guestEmail), {
          headers: { 'Content-Type': 'application/json' }
        })
        bookingToken = guestResponse.data.token
        localStorage.setItem('token', bookingToken)
      }

      const response = await axios.post('/api/bookings', {
        flightId: selectedFlight.id,
        passengers: passengers
      }, {
        headers: { Authorization: `Bearer ${bookingToken}` }
      })

      const pnr = response.data.bookingReference
      
      alert(
        '✅ Rezervasyon başarıyla tamamlandı!\n\n' +
        '🎫 PNR Numaranız: ' + pnr + '\n\n' +
        '💡 Bu numarayı not edin! Rezervasyonunuzu sorgulamak ve ödeme yapmak için kullanabilirsiniz.'
      )
      
      // Misafir kullanıcıysa üyelik teklif et
      if (!user && guestEmail) {
        const wantToRegister = confirm(
          '🎉 Rezervasyonunuz tamamlandı!\n\n' +
          '💡 Üye olarak daha fazla avantajdan yararlanabilirsiniz:\n' +
          '• Rezervasyon geçmişinizi görüntüleyin\n' +
          '• Hızlı rezervasyon yapın\n' +
          '• Özel kampanyalardan haberdar olun\n\n' +
          'Şimdi üye olmak ister misiniz?'
        )
        
        if (wantToRegister) {
          navigate('/register', { state: { email: guestEmail } })
          return
        }
      }
      
      setSelectedFlight(null)
      setPassengers([{
        firstName: '',
        lastName: '',
        identityNumber: '',
        dateOfBirth: '',
        gender: 'Erkek',
        seatNumber: '',
        seatType: ''
      }])
      setSelectedSeats([])
      setShowSeatSelector(false)
      
      if (user) {
        navigate('/bookings')
      } else {
        // Misafir kullanıcı için PNR ile ödeme yönlendirmesi
        const goToPayment = confirm(
          '💳 Ödeme yapmak ister misiniz?\n\n' +
          'PNR numaranızla ödeme sayfasına yönlendirileceksiniz.'
        )
        
        if (goToPayment) {
          navigate('/guest-booking', { state: { pnr: pnr, email: guestEmail } })
        } else {
          alert('ℹ️ PNR numaranızı not ettiniz: ' + pnr + '\n\nDaha sonra "PNR Sorgula" menüsünden ödeme yapabilirsiniz.')
          navigate('/')
        }
      }
    } catch (error) {
      const errorMsg = error.response?.data?.message || 'Rezervasyon yapılırken bir hata oluştu'
      alert('❌ ' + errorMsg)
    }
  }

  if (loading) return <div className="loading">Yükleniyor...</div>

  return (
    <div className="container">
      <div className="search-form">
        <h2 style={{ marginBottom: '24px' }}>Uçuş Ara</h2>
        <form onSubmit={handleSearch}>
          <div className="form-row">
            <input
              type="text"
              placeholder="Kalkış Şehri (örn: İstanbul)"
              value={searchParams.departureCity}
              onChange={(e) => setSearchParams({...searchParams, departureCity: e.target.value})}
            />
            <input
              type="text"
              placeholder="Varış Şehri (örn: Ankara)"
              value={searchParams.arrivalCity}
              onChange={(e) => setSearchParams({...searchParams, arrivalCity: e.target.value})}
            />
            <input
              type="date"
              value={searchParams.departureDate}
              onChange={(e) => setSearchParams({...searchParams, departureDate: e.target.value})}
            />
            <button type="submit" className="btn btn-primary">Ara</button>
          </div>
          <div style={{ marginTop: '10px', textAlign: 'center' }}>
            <button 
              type="button" 
              className="btn btn-secondary" 
              onClick={() => {
                setSearchParams({ departureCity: '', arrivalCity: '', departureDate: '' })
                setLoading(true)
                fetchFlights({})
              }}
            >
              Tüm Uçuşları Göster
            </button>
          </div>
        </form>
      </div>

      <div style={{ marginTop: '40px' }}>
        <h2 style={{ marginBottom: '24px', color: 'white' }}>Mevcut Uçuşlar ({flights.length})</h2>
        {flights.map(flight => (
          <div key={flight.id} className="flight-card" style={{ background: 'white' }}>
            <div className="flight-header">
              <div>
                <div className="flight-route">
                  {flight.departureCity} → {flight.arrivalCity}
                </div>
                <div style={{ color: '#666', fontSize: '14px' }}>{flight.airline} - {flight.flightNumber}</div>
              </div>
              <div className="flight-price">₺{flight.price}</div>
            </div>
            <div className="flight-details">
              <div className="detail-item">
                <span className="detail-label">Kalkış</span>
                <span className="detail-value">{new Date(flight.departureTime).toLocaleString('tr-TR')}</span>
              </div>
              <div className="detail-item">
                <span className="detail-label">Varış</span>
                <span className="detail-value">{new Date(flight.arrivalTime).toLocaleString('tr-TR')}</span>
              </div>
              <div className="detail-item">
                <span className="detail-label">Müsait Koltuk</span>
                <span className="detail-value">{flight.availableSeats} / {flight.totalSeats}</span>
              </div>
            </div>
            <button 
              className="btn btn-primary" 
              onClick={() => setSelectedFlight(flight)}
              disabled={flight.availableSeats === 0}
            >
              {flight.availableSeats === 0 ? 'Dolu' : 'Rezervasyon Yap'}
            </button>
          </div>
        ))}
      </div>

      {selectedFlight && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'rgba(0,0,0,0.5)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 1000
        }}>
          <div className="card" style={{ maxWidth: '600px', maxHeight: '90vh', overflow: 'auto' }}>
            <h2>Rezervasyon Detayları</h2>
            <p><strong>Uçuş:</strong> {selectedFlight.departureCity} → {selectedFlight.arrivalCity}</p>
            <p><strong>Fiyat:</strong> ₺{selectedFlight.price} x {passengers.length} = ₺{selectedFlight.price * passengers.length}</p>
            
            {!user && (
              <div style={{
                background: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
                color: 'white',
                padding: '20px',
                borderRadius: '12px',
                marginBottom: '20px'
              }}>
                <h3 style={{ fontSize: '18px', marginBottom: '12px' }}>✨ Hızlı Rezervasyon</h3>
                <p style={{ fontSize: '14px', marginBottom: '16px' }}>
                  Üye olmadan rezervasyon yapabilirsiniz! Sadece e-posta adresinizi girin.
                </p>
                <div className="form-group" style={{ marginBottom: '0' }}>
                  <label style={{ color: 'white' }}>E-posta Adresiniz</label>
                  <input
                    type="email"
                    value={guestEmail}
                    onChange={(e) => setGuestEmail(e.target.value)}
                    placeholder="ornek@email.com"
                    required
                    style={{ background: 'white' }}
                  />
                  <small style={{ color: 'rgba(255,255,255,0.9)', fontSize: '12px' }}>
                    💡 Rezervasyon bilgileriniz bu adrese gönderilecektir
                  </small>
                </div>
              </div>
            )}

            <form onSubmit={handleBooking}>
              {!showSeatSelector && (
                <div style={{ marginBottom: '20px', textAlign: 'center' }}>
                  <button 
                    type="button" 
                    className="btn btn-primary"
                    onClick={() => setShowSeatSelector(true)}
                    style={{ fontSize: '16px', padding: '12px 24px' }}
                  >
                    🪑 Koltuk Seç ({selectedSeats.length}/{passengers.length})
                  </button>
                </div>
              )}

              {showSeatSelector && (
                <div style={{ marginBottom: '20px' }}>
                  <SeatSelector 
                    flightId={selectedFlight.id}
                    passengerCount={passengers.length}
                    onSeatsSelected={handleSeatsSelected}
                  />
                  <button 
                    type="button" 
                    className="btn btn-secondary"
                    onClick={() => setShowSeatSelector(false)}
                    style={{ marginTop: '10px' }}
                  >
                    Kapat
                  </button>
                </div>
              )}

              {passengers.map((passenger, index) => (
                <div key={index} style={{ border: '1px solid #e0e0e0', padding: '16px', marginBottom: '16px', borderRadius: '8px' }}>
                  <h3>
                    Yolcu {index + 1}
                    {passenger.seatNumber && (
                      <span style={{ 
                        marginLeft: '10px', 
                        fontSize: '14px', 
                        background: 'linear-gradient(135deg, #667eea, #764ba2)',
                        color: 'white',
                        padding: '4px 12px',
                        borderRadius: '12px'
                      }}>
                        🪑 Koltuk: {passenger.seatNumber}
                      </span>
                    )}
                  </h3>
                  <input
                    type="text"
                    placeholder="Ad"
                    value={passenger.firstName}
                    onChange={(e) => handlePassengerChange(index, 'firstName', e.target.value)}
                    required
                  />
                  <input
                    type="text"
                    placeholder="Soyad"
                    value={passenger.lastName}
                    onChange={(e) => handlePassengerChange(index, 'lastName', e.target.value)}
                    required
                  />
                  <input
                    type="text"
                    placeholder="TC Kimlik No"
                    value={passenger.identityNumber}
                    onChange={(e) => handlePassengerChange(index, 'identityNumber', e.target.value)}
                    required
                  />
                  <input
                    type="date"
                    value={passenger.dateOfBirth}
                    onChange={(e) => handlePassengerChange(index, 'dateOfBirth', e.target.value)}
                    required
                  />
                  <select
                    value={passenger.gender}
                    onChange={(e) => handlePassengerChange(index, 'gender', e.target.value)}
                  >
                    <option value="Erkek">Erkek</option>
                    <option value="Kadın">Kadın</option>
                  </select>
                  {passengers.length > 1 && (
                    <button type="button" className="btn btn-danger" onClick={() => removePassenger(index)}>
                      Yolcuyu Kaldır
                    </button>
                  )}
                </div>
              ))}
              
              <button type="button" className="btn btn-secondary" onClick={addPassenger} style={{ marginRight: '10px' }}>
                Yolcu Ekle
              </button>
              <button type="submit" className="btn btn-success" style={{ marginRight: '10px' }}>
                Rezervasyonu Tamamla
              </button>
              <button type="button" className="btn btn-secondary" onClick={() => setSelectedFlight(null)}>
                İptal
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}

export default Flights
