import { useState, useEffect, useMemo } from 'react'
import axios from 'axios'
import { useNavigate } from 'react-router-dom'
import SeatSelector from '../components/SeatSelector'

// Türkiye havalimanları mapping
const airportMapping = {
  'İstanbul': { code: 'IST', name: 'İstanbul Havalimanı' },
  'Ankara': { code: 'ESB', name: 'Esenboğa Havalimanı' },
  'İzmir': { code: 'ADB', name: 'Adnan Menderes Havalimanı' },
  'Antalya': { code: 'AYT', name: 'Antalya Havalimanı' },
  'Adana': { code: 'ADA', name: 'Şakirpaşa Havalimanı' },
  'Trabzon': { code: 'TZX', name: 'Trabzon Havalimanı' },
  'Gaziantep': { code: 'GZT', name: 'Oğuzeli Havalimanı' },
  'Kayseri': { code: 'ASR', name: 'Erkilet Havalimanı' },
  'Konya': { code: 'KYA', name: 'Konya Havalimanı' },
  'Bursa': { code: 'YEI', name: 'Yenişehir Havalimanı' },
  'Diyarbakır': { code: 'DIY', name: 'Diyarbakır Havalimanı' },
  'Erzurum': { code: 'ERZ', name: 'Erzurum Havalimanı' },
  'Samsun': { code: 'SZF', name: 'Çarşamba Havalimanı' },
  'Denizli': { code: 'DNZ', name: 'Çardak Havalimanı' },
  'Bodrum': { code: 'BJV', name: 'Milas-Bodrum Havalimanı' },
  'Dalaman': { code: 'DLM', name: 'Dalaman Havalimanı' }
}

// Şehir bilgilerini formatla
const formatCityInfo = (cityName) => {
  const airport = airportMapping[cityName]
  if (airport) {
    return `${cityName} (${airport.code}) - ${airport.name}`
  }
  return cityName
}

function Flights({ user }) {
  const [flights, setFlights] = useState([])
  const [allFlights, setAllFlights] = useState([])
  const [loading, setLoading] = useState(true)
  const [searchParams, setSearchParams] = useState({
    departureCity: '',
    arrivalCity: '',
    departureDate: ''
  })
  const [cities, setCities] = useState({ departure: [], arrival: [] })
  const [showAlternatives, setShowAlternatives] = useState(false)
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
  const [showConfirmation, setShowConfirmation] = useState(false)
  const [showSuccessModal, setShowSuccessModal] = useState(false)
  const [completedBooking, setCompletedBooking] = useState(null)
  const navigate = useNavigate()

  useEffect(() => {
    fetchFlights()
  }, [])

  // Modal açıldığında body scroll'unu engelle ve modal'ı en üste kaydır
  useEffect(() => {
    if (selectedFlight) {
      document.body.style.overflow = 'hidden'
      // Modal açıldığında scroll'u en üste getir
      setTimeout(() => {
        const modalContainer = document.querySelector('[data-modal-container]')
        if (modalContainer) {
          modalContainer.scrollTop = 0
        }
      }, 0)
    } else {
      document.body.style.overflow = 'unset'
    }
    
    // Cleanup function
    return () => {
      document.body.style.overflow = 'unset'
    }
  }, [selectedFlight])

  const fetchFlights = async (params = {}) => {
    try {
      // Boş parametreleri filtrele
      const filteredParams = {}
      if (params.departureCity) filteredParams.departureCity = params.departureCity
      if (params.arrivalCity) filteredParams.arrivalCity = params.arrivalCity
      if (params.departureDate) filteredParams.departureDate = params.departureDate
      
      const response = await axios.get('/api/flights', { params: filteredParams })
      
      // İlk yüklemede tüm uçuşları sakla ve şehir listesini oluştur
      if (Object.keys(params).length === 0) {
        setAllFlights(response.data)
        const departureCities = [...new Set(response.data.map(f => f.departureCity))].sort()
        const arrivalCities = [...new Set(response.data.map(f => f.arrivalCity))].sort()
        setCities({ departure: departureCities, arrival: arrivalCities })
      }
      
      setFlights(response.data)
      setShowAlternatives(false)
    } catch (error) {
      console.error('Uçuşlar yüklenemedi:', error)
    } finally {
      setLoading(false)
    }
  }

  const findAlternativeFlights = () => {
    if (!searchParams.departureCity && !searchParams.arrivalCity) return []
    
    let alternatives = []
    
    // Her iki şehir seçilmişse
    if (searchParams.departureCity && searchParams.arrivalCity) {
      // Önce aynı rotadaki ileri tarihli uçuşları bul
      const sameRouteFlights = allFlights.filter(flight => 
        flight.departureCity === searchParams.departureCity && 
        flight.arrivalCity === searchParams.arrivalCity
      )
      
      // Tarih seçilmişse, o tarihten sonraki uçuşları göster
      if (searchParams.departureDate) {
        const searchDate = new Date(searchParams.departureDate)
        const futureFlights = sameRouteFlights.filter(flight => {
          const flightDate = new Date(flight.departureTime)
          return flightDate > searchDate
        })
        
        if (futureFlights.length > 0) {
          alternatives = futureFlights
        }
      }
      
      // Aynı rotada ileri tarihli uçuş yoksa, girilen şehirlerden kalkan diğer uçuşları göster
      if (alternatives.length === 0) {
        alternatives = allFlights.filter(flight => 
          flight.departureCity === searchParams.departureCity || 
          flight.departureCity === searchParams.arrivalCity
        )
      }
    }
    // Sadece kalkış şehri seçilmişse
    else if (searchParams.departureCity && !searchParams.arrivalCity) {
      alternatives = allFlights.filter(flight => 
        flight.departureCity === searchParams.departureCity
      )
    }
    // Sadece varış şehri seçilmişse
    else if (searchParams.arrivalCity && !searchParams.departureCity) {
      alternatives = allFlights.filter(flight => 
        flight.departureCity === searchParams.arrivalCity
      )
    }
    
    // Tarihe göre sıralama
    if (searchParams.departureDate) {
      // Belirli bir tarih seçilmişse, o tarihe en yakın uçuşları göster
      const searchDate = new Date(searchParams.departureDate)
      alternatives.sort((a, b) => {
        const dateA = Math.abs(new Date(a.departureTime) - searchDate)
        const dateB = Math.abs(new Date(b.departureTime) - searchDate)
        return dateA - dateB
      })
    } else {
      // Tarih seçilmemişse, en yakın tarihli uçuşları göster
      const now = new Date()
      alternatives.sort((a, b) => {
        const dateA = new Date(a.departureTime)
        const dateB = new Date(b.departureTime)
        // Geçmiş uçuşları filtrele
        if (dateA < now && dateB >= now) return 1
        if (dateB < now && dateA >= now) return -1
        if (dateA < now && dateB < now) return dateB - dateA // Geçmiş uçuşlar için ters sıra
        return dateA - dateB // Gelecek uçuşlar için normal sıra
      })
      // Geçmiş uçuşları filtrele
      alternatives = alternatives.filter(flight => new Date(flight.departureTime) >= now)
    }
    
    return alternatives.slice(0, 5) // En fazla 5 alternatif göster
  }

  const handleSearch = async (e) => {
    e.preventDefault()
    setLoading(true)
    
    try {
      const filteredParams = {}
      if (searchParams.departureCity) filteredParams.departureCity = searchParams.departureCity
      if (searchParams.arrivalCity) filteredParams.arrivalCity = searchParams.arrivalCity
      if (searchParams.departureDate) filteredParams.departureDate = searchParams.departureDate
      
      const response = await axios.get('/api/flights', { params: filteredParams })
      setFlights(response.data)
      
      // Arama sonucu boşsa alternatif uçuşları göster
      if (response.data.length === 0) {
        setShowAlternatives(true)
      } else {
        setShowAlternatives(false)
      }
    } catch (error) {
      console.error('Uçuşlar yüklenemedi:', error)
    } finally {
      setLoading(false)
    }
  }

  // TC Kimlik numarası doğrulama fonksiyonu
  const validateTCKimlik = (tcNo) => {
    if (!tcNo || tcNo.length !== 11) return false
    
    // İlk hane 0 olamaz
    if (tcNo[0] === '0') return false
    
    // Tüm haneler aynı olamaz
    if (tcNo.split('').every(digit => digit === tcNo[0])) return false
    
    // TC Kimlik algoritması
    const digits = tcNo.split('').map(Number)
    
    // İlk 10 hanenin toplamının son hanesi, 11. haneye eşit olmalı
    const sum = digits.slice(0, 10).reduce((acc, digit) => acc + digit, 0)
    if (sum % 10 !== digits[10]) return false
    
    // 1,3,5,7,9. hanelerin toplamının 7 katı ile 2,4,6,8. hanelerin toplamının farkının son hanesi 10. haneye eşit olmalı
    const oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8]
    const evenSum = digits[1] + digits[3] + digits[5] + digits[7]
    if ((oddSum * 7 - evenSum) % 10 !== digits[9]) return false
    
    return true
  }

  // Aynı TC'nin başka yolcu tarafından kullanılıp kullanılmadığını kontrol et
  const checkDuplicateTC = (tcNo, currentIndex) => {
    return passengers.some((passenger, index) => 
      index !== currentIndex && passenger.identityNumber === tcNo
    )
  }

  const handlePassengerChange = (index, field, value) => {
    const newPassengers = [...passengers]
    
    // TC kimlik numarası için özel kontrol
    if (field === 'identityNumber') {
      // Sadece rakam kabul et
      const numericValue = value.replace(/\D/g, '')
      if (numericValue.length <= 11) {
        newPassengers[index][field] = numericValue
        
        // 11 hane tamamlandığında doğrulama yap
        if (numericValue.length === 11) {
          // TC kimlik doğrulaması
          if (!validateTCKimlik(numericValue)) {
            alert('❌ Geçersiz TC Kimlik numarası! Lütfen doğru TC Kimlik numaranızı girin.')
            return
          }
          
          // Aynı TC kontrolü
          if (checkDuplicateTC(numericValue, index)) {
            alert('❌ Bu TC Kimlik numarası başka bir yolcu tarafından kullanılıyor!')
            return
          }
        }
      }
    } else {
      newPassengers[index][field] = value
    }
    
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
      setShowConfirmation(false)
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
    // Yolcu sayısı kadar koltuk seçilmediğinde uyarı ver
    if (seats.length !== passengers.length) {
      alert(`⚠️ Lütfen ${passengers.length} koltuk seçin. Şu anda ${seats.length} koltuk seçili.`)
      return
    }

    setSelectedSeats(seats)
    
    // Koltukları yolculara ata
    const updatedPassengers = passengers.map((passenger, index) => ({
      ...passenger,
      seatNumber: seats[index]?.seatNumber || '',
      seatType: seats[index]?.seatType || ''
    }))
    setPassengers(updatedPassengers)
    
    // Koltuk seçimi onaylandıktan sonra selector'ı kapat
    setShowSeatSelector(false)
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
      setShowConfirmation(false)
    }
    
    setPassengers(passengers.filter((_, i) => i !== index))
  }

  const handleReservationConfirm = (e) => {
    e.preventDefault()
    
    if (!user && !guestEmail) {
      alert('⚠️ Lütfen giriş yapın veya misafir e-posta adresi girin')
      return
    }

    // Koltuk seçimi kontrolü
    if (selectedSeats.length !== passengers.length) {
      alert('⚠️ Lütfen tüm yolcular için koltuk seçin')
      return
    }

    // TC kimlik numarası kontrolü
    const invalidTcNumbers = passengers.filter(p => p.identityNumber.length !== 11)
    if (invalidTcNumbers.length > 0) {
      alert('⚠️ Tüm yolcular için geçerli TC Kimlik numarası (11 hane) girilmelidir')
      return
    }

    // TC kimlik numarası doğrulama kontrolü
    const invalidTcValidation = passengers.filter(p => !validateTCKimlik(p.identityNumber))
    if (invalidTcValidation.length > 0) {
      alert('⚠️ Geçersiz TC Kimlik numarası girdiniz. Lütfen doğru TC Kimlik numaralarını girin.')
      return
    }

    // Aynı TC kimlik numarası kontrolü
    const tcNumbers = passengers.map(p => p.identityNumber)
    const duplicateTCs = tcNumbers.filter((tc, index) => tcNumbers.indexOf(tc) !== index)
    if (duplicateTCs.length > 0) {
      alert('⚠️ Aynı TC Kimlik numarası birden fazla yolcu için kullanılamaz!')
      return
    }

    // Tüm kontroller geçtiyse onay ekranını göster
    setShowConfirmation(true)
  }

  const handleBooking = async (e) => {
    e.preventDefault()
    
    // Bu fonksiyon sadece onay ekranından çağrılır, kontroller zaten yapıldı
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

      console.log('Gönderilen veri:', {
        flightId: selectedFlight.id,
        passengers: passengers
      })

      const response = await axios.post('/api/bookings', {
        flightId: selectedFlight.id,
        passengers: passengers
      }, {
        headers: { Authorization: `Bearer ${bookingToken}` }
      })

      const pnr = response.data.bookingReference
      
      // Başarı modal'ını göster
      setCompletedBooking({
        pnr: pnr,
        email: user?.email || guestEmail,
        flight: selectedFlight,
        passengerCount: passengers.length
      })
      setShowSuccessModal(true)
      
      // Form verilerini temizle
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
      setShowConfirmation(false)
    } catch (error) {
      console.error('Rezervasyon hatası:', error)
      console.error('Hata detayı:', error.response?.data)
      const errorMsg = error.response?.data?.message || error.message || 'Rezervasyon yapılırken bir hata oluştu'
      alert('❌ ' + errorMsg)
    }
  }

  if (loading) return <div className="loading">Yükleniyor...</div>

  return (
    <div className="container">
      <div className="search-form">
        <h2 style={{ marginBottom: '32px', fontSize: '32px', fontWeight: '800', color: '#37474f' }}>
          BULUTBİLET<span style={{ color: '#00e5ff' }}>.COM</span> - Uçuş Ara
        </h2>
        <form onSubmit={handleSearch}>
          <div className="form-row">
            <select
              value={searchParams.departureCity}
              onChange={(e) => {
                const newDepartureCity = e.target.value
                // Eğer varış şehri ile aynıysa varış şehrini temizle
                if (newDepartureCity === searchParams.arrivalCity) {
                  setSearchParams({...searchParams, departureCity: newDepartureCity, arrivalCity: ''})
                } else {
                  setSearchParams({...searchParams, departureCity: newDepartureCity})
                }
              }}
              style={{ padding: '12px', borderRadius: '8px', border: '1px solid #ddd' }}
            >
              <option value="">Kalkış Şehri Seçin</option>
              {cities.departure.map(city => (
                <option key={city} value={city}>
                  {city} ({airportMapping[city]?.code || 'N/A'}) - {airportMapping[city]?.name || city}
                </option>
              ))}
            </select>
            <select
              value={searchParams.arrivalCity}
              onChange={(e) => {
                const newArrivalCity = e.target.value
                // Eğer kalkış şehri ile aynıysa uyarı ver ve seçimi engelle
                if (newArrivalCity === searchParams.departureCity) {
                  alert('⚠️ Kalkış ve varış şehri aynı olamaz!')
                  return
                }
                setSearchParams({...searchParams, arrivalCity: newArrivalCity})
              }}
              style={{ padding: '12px', borderRadius: '8px', border: '1px solid #ddd' }}
            >
              <option value="">Varış Şehri Seçin</option>
              {cities.arrival.map(city => (
                <option 
                  key={city} 
                  value={city}
                  disabled={city === searchParams.departureCity}
                  style={{ 
                    color: city === searchParams.departureCity ? '#ccc' : 'inherit',
                    fontStyle: city === searchParams.departureCity ? 'italic' : 'normal'
                  }}
                >
                  {city === searchParams.departureCity 
                    ? `${city} (Kalkış şehri)` 
                    : `${city} (${airportMapping[city]?.code || 'N/A'}) - ${airportMapping[city]?.name || city}`
                  }
                </option>
              ))}
            </select>
            <input
              type="date"
              value={searchParams.departureDate}
              onChange={(e) => setSearchParams({...searchParams, departureDate: e.target.value})}
              min={new Date().toISOString().split('T')[0]}
              max={new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]}
              style={{ padding: '12px', borderRadius: '8px', border: '1px solid #ddd' }}
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
        <h2 style={{ marginBottom: '24px', color: 'white' }}>
          {flights.length > 0 ? `Mevcut Uçuşlar (${flights.length})` : 'Aradığınız Kriterlerde Uçuş Bulunamadı'}
        </h2>
        
        {flights.length === 0 && showAlternatives && (
          <div style={{ 
            background: 'linear-gradient(135deg, #00bcd4 0%, #00acc1 100%)',
            color: 'white',
            padding: '24px',
            borderRadius: '16px',
            marginBottom: '24px'
          }}>
            <h3 style={{ fontSize: '18px', marginBottom: '12px' }}>✈️ Alternatif Uçuşlar</h3>
            <p style={{ fontSize: '14px', marginBottom: '16px' }}>
              {(() => {
                const sameRouteAlternatives = findAlternativeFlights().filter(flight => 
                  flight.departureCity === searchParams.departureCity && 
                  flight.arrivalCity === searchParams.arrivalCity
                )
                
                if (searchParams.departureDate && sameRouteAlternatives.length > 0) {
                  return `${searchParams.departureDate} tarihinde uçuş bulunamadı. ${searchParams.departureCity} - ${searchParams.arrivalCity} rotasındaki ileri tarihli uçuşlar:`
                } else if (searchParams.departureCity && searchParams.arrivalCity) {
                  return `${searchParams.departureCity} - ${searchParams.arrivalCity} rotasında uçuş bulunamadı. Alternatif uçuşlar:`
                } else if (searchParams.departureCity) {
                  return `${searchParams.departureCity} şehrinden kalkan alternatif uçuşlar:`
                } else {
                  return `${searchParams.arrivalCity} şehrinden kalkan alternatif uçuşlar:`
                }
              })()}
            </p>
            {findAlternativeFlights().map(flight => (
              <div key={flight.id} className="flight-card alternative-flight" style={{ 
                background: 'rgba(255,255,255,0.1)', 
                border: '1px solid rgba(255,255,255,0.2)',
                marginBottom: '12px'
              }}>
                <div className="flight-header">
                  <div>
                    <div className="flight-route" style={{ color: 'white' }}>
                      <div style={{ fontSize: '18px', fontWeight: 'bold', marginBottom: '4px' }}>
                        {flight.departureCity} → {flight.arrivalCity}
                      </div>
                      <div style={{ fontSize: '12px', color: 'rgba(255,255,255,0.7)' }}>
                        {airportMapping[flight.departureCity]?.code || 'N/A'} → {airportMapping[flight.arrivalCity]?.code || 'N/A'}
                      </div>
                      <div style={{ fontSize: '11px', color: 'rgba(255,255,255,0.6)', marginTop: '2px' }}>
                        {airportMapping[flight.departureCity]?.name || flight.departureCity} → {airportMapping[flight.arrivalCity]?.name || flight.arrivalCity}
                      </div>
                    </div>
                    <div style={{ color: 'rgba(255,255,255,0.8)', fontSize: '14px' }}>
                      {flight.airline} - {flight.flightNumber}
                    </div>
                  </div>
                  <div className="flight-price" style={{ color: 'white' }}>₺{flight.price}</div>
                </div>
                <div className="flight-details">
                  <div className="detail-item">
                    <span className="detail-label" style={{ color: 'rgba(255,255,255,0.8)' }}>Kalkış Tarihi</span>
                    <span className="detail-value" style={{ color: 'white', fontWeight: 'bold' }}>
                      {new Date(flight.departureTime).toLocaleDateString('tr-TR')} - {new Date(flight.departureTime).toLocaleTimeString('tr-TR', {hour: '2-digit', minute: '2-digit'})}
                    </span>
                  </div>
                  <div className="detail-item">
                    <span className="detail-label" style={{ color: 'rgba(255,255,255,0.8)' }}>Müsait Koltuk</span>
                    <span className="detail-value" style={{ color: 'white' }}>
                      {flight.availableSeats} / {flight.totalSeats}
                    </span>
                  </div>
                </div>
                <button 
                  className="btn btn-primary" 
                  onClick={() => setSelectedFlight(flight)}
                  disabled={flight.availableSeats === 0}
                  style={{ background: 'white', color: '#667eea' }}
                >
                  {flight.availableSeats === 0 ? 'Dolu' : 'Bu Uçuşu Seç'}
                </button>
              </div>
            ))}
          </div>
        )}
        
        {flights.map(flight => (
          <div key={flight.id} className="flight-card" style={{ background: 'white' }}>
            <div className="flight-header">
              <div>
                <div className="flight-route">
                  <div style={{ fontSize: '18px', fontWeight: 'bold', marginBottom: '4px' }}>
                    {flight.departureCity} → {flight.arrivalCity}
                  </div>
                  <div style={{ fontSize: '12px', color: '#666' }}>
                    {airportMapping[flight.departureCity]?.code || 'N/A'} → {airportMapping[flight.arrivalCity]?.code || 'N/A'}
                  </div>
                  <div style={{ fontSize: '11px', color: '#888', marginTop: '2px' }}>
                    {airportMapping[flight.departureCity]?.name || flight.departureCity} → {airportMapping[flight.arrivalCity]?.name || flight.arrivalCity}
                  </div>
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
        <div 
          data-modal-container
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            width: '100vw',
            height: '100vh',
            background: 'rgba(0,0,0,0.8)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 9999,
            padding: '0',
            margin: '0',
            boxSizing: 'border-box',
            overflow: 'hidden'
          }}>
          <div className="card" style={{ 
            width: '100vw',
            height: '100vh',
            maxWidth: 'none', 
            maxHeight: 'none', 
            overflow: 'auto',
            margin: '0',
            padding: '0',
            boxSizing: 'border-box',
            position: 'relative',
            backgroundColor: 'white',
            borderRadius: '0',
            boxShadow: 'none'
          }}>
            <div style={{ padding: '20px' }}>
            <h2 className="modal-title">Rezervasyon Detayları</h2>
            <div className="flight-info-box">
              <p className="flight-info-item">
                <strong>Uçuş:</strong> {selectedFlight.departureCity} ({airportMapping[selectedFlight.departureCity]?.code || 'N/A'}) → {selectedFlight.arrivalCity} ({airportMapping[selectedFlight.arrivalCity]?.code || 'N/A'})
              </p>
              <p className="flight-info-item" style={{ fontSize: '12px', color: '#666', marginTop: '4px' }}>
                {airportMapping[selectedFlight.departureCity]?.name || selectedFlight.departureCity} → {airportMapping[selectedFlight.arrivalCity]?.name || selectedFlight.arrivalCity}
              </p>
              <p className="flight-info-item"><strong>Uçuş No:</strong> {selectedFlight.flightNumber} - {selectedFlight.airline}</p>
              <p className="flight-info-item"><strong>Kalkış Tarihi:</strong> {new Date(selectedFlight.departureTime).toLocaleDateString('tr-TR')}</p>
              <p className="flight-info-item"><strong>Kalkış Saati:</strong> {new Date(selectedFlight.departureTime).toLocaleTimeString('tr-TR', {hour: '2-digit', minute: '2-digit'})}</p>
              <p className="flight-info-item-last"><strong>Fiyat:</strong> ₺{selectedFlight.price} x {passengers.length} = ₺{selectedFlight.price * passengers.length}</p>
            </div>
            
            {!user && (
              <div style={{
                background: 'linear-gradient(135deg, #00bcd4 0%, #00acc1 100%)',
                color: 'white',
                padding: '24px',
                borderRadius: '16px',
                marginBottom: '24px'
              }}>
                <h3 style={{ fontSize: '20px', marginBottom: '16px', fontWeight: '700' }}>✨ Hızlı Rezervasyon</h3>
                <p style={{ fontSize: '16px', marginBottom: '20px', lineHeight: '1.6' }}>
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

            <form onSubmit={handleReservationConfirm}>

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
                  <div className="form-group">
                    <label>Ad</label>
                    <input
                      type="text"
                      placeholder="Ad"
                      value={passenger.firstName}
                      onChange={(e) => handlePassengerChange(index, 'firstName', e.target.value)}
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label>Soyad</label>
                    <input
                      type="text"
                      placeholder="Soyad"
                      value={passenger.lastName}
                      onChange={(e) => handlePassengerChange(index, 'lastName', e.target.value)}
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label>TC Kimlik No</label>
                    <input
                      type="text"
                      placeholder="TC Kimlik No (11 hane)"
                      value={passenger.identityNumber}
                      onChange={(e) => handlePassengerChange(index, 'identityNumber', e.target.value)}
                      maxLength="11"
                      minLength="11"
                      pattern="[0-9]{11}"
                      required
                      style={{
                        borderColor: passenger.identityNumber && passenger.identityNumber.length !== 11 ? '#dc3545' : 
                                   passenger.identityNumber && passenger.identityNumber.length === 11 && validateTCKimlik(passenger.identityNumber) ? '#28a745' : ''
                      }}
                    />
                    {passenger.identityNumber && passenger.identityNumber.length > 0 && passenger.identityNumber.length !== 11 && (
                      <small style={{ color: '#dc3545', fontSize: '12px', display: 'block', marginTop: '4px' }}>
                        ❌ TC Kimlik numarası 11 hane olmalıdır ({passenger.identityNumber.length}/11)
                      </small>
                    )}
                    {passenger.identityNumber && passenger.identityNumber.length === 11 && !validateTCKimlik(passenger.identityNumber) && (
                      <small style={{ color: '#dc3545', fontSize: '12px', display: 'block', marginTop: '4px' }}>
                        ❌ Geçersiz TC Kimlik numarası
                      </small>
                    )}
                    {passenger.identityNumber && passenger.identityNumber.length === 11 && validateTCKimlik(passenger.identityNumber) && !checkDuplicateTC(passenger.identityNumber, index) && (
                      <small style={{ color: '#28a745', fontSize: '12px', display: 'block', marginTop: '4px' }}>
                        ✅ TC Kimlik numarası geçerli
                      </small>
                    )}
                    {passenger.identityNumber && passenger.identityNumber.length === 11 && checkDuplicateTC(passenger.identityNumber, index) && (
                      <small style={{ color: '#dc3545', fontSize: '12px', display: 'block', marginTop: '4px' }}>
                        ❌ Bu TC Kimlik numarası başka bir yolcu tarafından kullanılıyor
                      </small>
                    )}
                  </div>
                  <div className="form-group">
                    <label>Doğum Tarihi</label>
                    <input
                      type="date"
                      value={passenger.dateOfBirth}
                      onChange={(e) => handlePassengerChange(index, 'dateOfBirth', e.target.value)}
                      max={new Date().toISOString().split('T')[0]}
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label>Cinsiyet</label>
                    <select
                      value={passenger.gender}
                      onChange={(e) => handlePassengerChange(index, 'gender', e.target.value)}
                    >
                      <option value="Erkek">Erkek</option>
                      <option value="Kadın">Kadın</option>
                    </select>
                  </div>
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

              {!showSeatSelector && !showConfirmation && (
                <div style={{ margin: '20px 0', textAlign: 'center' }}>
                  <button 
                    type="button" 
                    className={`btn ${selectedSeats.length === passengers.length ? 'btn-success' : 'btn-primary'}`}
                    onClick={() => setShowSeatSelector(true)}
                    style={{ fontSize: '16px', padding: '12px 24px', width: '100%' }}
                  >
                    {selectedSeats.length === passengers.length 
                      ? `✅ Koltuklar Seçildi (${selectedSeats.length}/${passengers.length})` 
                      : `🪑 Koltuk Seç (${selectedSeats.length}/${passengers.length})`
                    }
                  </button>
                </div>
              )}

              {showSeatSelector && (
                <div style={{ margin: '20px 0' }}>
                  <SeatSelector 
                    flightId={selectedFlight.id}
                    passengerCount={passengers.length}
                    onSeatsSelected={handleSeatsSelected}
                  />
                  <button 
                    type="button" 
                    className="btn btn-secondary"
                    onClick={() => setShowSeatSelector(false)}
                    style={{ marginTop: '10px', width: '100%' }}
                  >
                    Kapat
                  </button>
                </div>
              )}

              {showConfirmation && (
                <div style={{
                  margin: '24px 0',
                  padding: '32px',
                  background: 'linear-gradient(135deg, #4caf50 0%, #388e3c 100%)',
                  color: 'white',
                  borderRadius: '16px'
                }}>
                  <h3 style={{ fontSize: '24px', marginBottom: '20px', textAlign: 'center', fontWeight: '700' }}>
                    ✅ Koltuk Seçimi Tamamlandı!
                  </h3>
                  
                  <div style={{ marginBottom: '20px' }}>
                    <h4 style={{ fontSize: '16px', marginBottom: '12px' }}>📋 Rezervasyon Özeti:</h4>
                    <div style={{ background: 'rgba(255,255,255,0.1)', padding: '16px', borderRadius: '8px' }}>
                      <p style={{ margin: '4px 0' }}>
                        <strong>Uçuş:</strong> {selectedFlight.flightNumber} - {selectedFlight.departureCity} ({airportMapping[selectedFlight.departureCity]?.code}) → {selectedFlight.arrivalCity} ({airportMapping[selectedFlight.arrivalCity]?.code})
                      </p>
                      <p style={{ margin: '2px 0', fontSize: '12px', color: 'rgba(255,255,255,0.8)' }}>
                        {airportMapping[selectedFlight.departureCity]?.name} → {airportMapping[selectedFlight.arrivalCity]?.name}
                      </p>
                      <p style={{ margin: '4px 0' }}><strong>Tarih:</strong> {new Date(selectedFlight.departureTime).toLocaleDateString('tr-TR')}</p>
                      <p style={{ margin: '4px 0' }}><strong>Yolcu Sayısı:</strong> {passengers.length}</p>
                      <p style={{ margin: '4px 0' }}><strong>Toplam Tutar:</strong> ₺{selectedFlight.price * passengers.length}</p>
                    </div>
                  </div>

                  <div style={{ marginBottom: '20px' }}>
                    <h4 style={{ fontSize: '16px', marginBottom: '12px' }}>🪑 Seçilen Koltuklar:</h4>
                    <div style={{ display: 'grid', gap: '8px' }}>
                      {selectedSeats.map((seat, index) => (
                        <div key={seat.seatNumber} style={{
                          background: 'rgba(255,255,255,0.1)',
                          padding: '8px 12px',
                          borderRadius: '6px',
                          display: 'flex',
                          justifyContent: 'space-between',
                          alignItems: 'center'
                        }}>
                          <span><strong>Yolcu {index + 1}:</strong> {passengers[index]?.firstName} {passengers[index]?.lastName}</span>
                          <span>🪑 {seat.seatNumber} ({seat.seatType === 'Window' ? '🪟 Cam' : seat.seatType === 'Aisle' ? '🚶 Koridor' : '💺 Orta'})</span>
                        </div>
                      ))}
                    </div>
                  </div>

                  <div style={{ textAlign: 'center' }}>
                    <p style={{ fontSize: '14px', marginBottom: '16px' }}>
                      Bilgileri kontrol edin ve rezervasyonunuzu onaylayın.
                    </p>
                    <div style={{ display: 'flex', gap: '12px', justifyContent: 'center' }}>
                      <button 
                        type="button" 
                        className="btn btn-success"
                        onClick={handleBooking}
                        style={{ fontSize: '16px', padding: '12px 24px' }}
                      >
                        ✅ Rezervasyonu Onayla
                      </button>
                      <button 
                        type="button" 
                        className="btn btn-secondary"
                        onClick={() => {
                          setShowConfirmation(false)
                          setShowSeatSelector(true)
                        }}
                        style={{ fontSize: '16px', padding: '12px 24px' }}
                      >
                        🔄 Koltuk Değiştir
                      </button>
                    </div>
                  </div>
                </div>
              )}

              <button 
                type="submit" 
                className="btn btn-success" 
                style={{ 
                  marginRight: '10px',
                  opacity: selectedSeats.length === passengers.length ? 1 : 0.6,
                  cursor: selectedSeats.length === passengers.length ? 'pointer' : 'not-allowed'
                }}
                disabled={selectedSeats.length !== passengers.length}
              >
                {selectedSeats.length === passengers.length ? '🎫 Rezervasyonu Tamamla' : 'Önce Koltuk Seçin'}
              </button>
              <button type="button" className="btn btn-secondary" onClick={() => {
                setSelectedFlight(null)
                setShowConfirmation(false)
                setSelectedSeats([])
                setShowSeatSelector(false)
              }}>
                İptal
              </button>
            </form>
            </div>
          </div>
        </div>
      )}

      {/* Başarı Modal */}
      {showSuccessModal && completedBooking && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          width: '100vw',
          height: '100vh',
          background: 'rgba(0,0,0,0.8)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 10000,
          padding: '20px',
          boxSizing: 'border-box'
        }}>
          <div style={{
            background: 'white',
            borderRadius: '20px',
            padding: '40px',
            maxWidth: '500px',
            width: '100%',
            textAlign: 'center',
            boxShadow: '0 20px 60px rgba(0,0,0,0.3)'
          }}>
            <div style={{
              background: 'linear-gradient(135deg, #4caf50 0%, #388e3c 100%)',
              color: 'white',
              padding: '24px',
              borderRadius: '20px',
              marginBottom: '32px'
            }}>
              <div style={{ fontSize: '56px', marginBottom: '16px' }}>🎉</div>
              <h2 style={{ fontSize: '28px', marginBottom: '12px', fontWeight: '800' }}>Rezervasyon Tamamlandı!</h2>
              <p style={{ fontSize: '18px', fontWeight: '500' }}>Biletiniz başarıyla rezerve edildi</p>
            </div>

            <div style={{
              background: '#f8f9ff',
              padding: '20px',
              borderRadius: '12px',
              marginBottom: '24px',
              textAlign: 'left'
            }}>
              <h3 style={{ marginBottom: '16px', color: '#333' }}>📋 Rezervasyon Bilgileri</h3>
              <div style={{ marginBottom: '12px' }}>
                <strong>🎫 PNR Numarası:</strong> 
                <span style={{ 
                  marginLeft: '8px', 
                  fontSize: '18px', 
                  fontWeight: 'bold', 
                  color: '#667eea',
                  background: '#e8ecff',
                  padding: '4px 12px',
                  borderRadius: '6px',
                  display: 'inline-block'
                }}>
                  {completedBooking.pnr}
                </span>
              </div>
              <div style={{ marginBottom: '12px' }}>
                <strong>📧 E-posta:</strong> {completedBooking.email}
              </div>
              <div style={{ marginBottom: '12px' }}>
                <strong>✈️ Uçuş:</strong> {completedBooking.flight.flightNumber} - {completedBooking.flight.departureCity} → {completedBooking.flight.arrivalCity}
              </div>
              <div>
                <strong>👥 Yolcu Sayısı:</strong> {completedBooking.passengerCount}
              </div>
            </div>

            <div style={{
              background: '#fff3cd',
              border: '1px solid #ffeaa7',
              color: '#856404',
              padding: '16px',
              borderRadius: '8px',
              marginBottom: '24px',
              fontSize: '14px'
            }}>
              💡 Rezervasyonunuz onaylandı. Ödeme yapmak için PNR sorgulama sayfasını kullanabilirsiniz.
            </div>

            <div style={{ display: 'flex', gap: '12px', justifyContent: 'center' }}>
              <button 
                className="btn btn-primary"
                onClick={() => {
                  navigate(`/guest-booking?pnr=${completedBooking.pnr}&email=${encodeURIComponent(completedBooking.email)}`)
                  setShowSuccessModal(false)
                  setCompletedBooking(null)
                }}
                style={{ fontSize: '16px', padding: '12px 24px' }}
              >
                🎫 PNR Göster & Ödeme Yap
              </button>
              <button 
                className="btn btn-secondary"
                onClick={() => {
                  setShowSuccessModal(false)
                  setCompletedBooking(null)
                  navigate('/')
                }}
                style={{ fontSize: '16px', padding: '12px 24px' }}
              >
                🏠 Ana Sayfa
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default Flights
