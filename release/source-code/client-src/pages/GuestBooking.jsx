import { useState, useEffect } from 'react'
import { useLocation, useSearchParams } from 'react-router-dom'
import axios from 'axios'

// Rezervasyon durumu çevirisi
const getStatusText = (status, isPaid) => {
  if (status === 'Cancelled') return 'İptal Edildi'
  if (status === 'Confirmed' && isPaid) return 'Ödendi'
  if (status === 'Confirmed' && !isPaid) return 'Onaylandı'
  return status
}

// E-posta ile sorgulama bileşeni
function EmailSearchForm({ onBookingsFound }) {
  const [searchEmail, setSearchEmail] = useState('')
  const [emailLoading, setEmailLoading] = useState(false)
  const [emailError, setEmailError] = useState('')

  const handleEmailSearch = async (e) => {
    e.preventDefault()
    setEmailError('')
    setEmailLoading(true)

    try {
      const response = await axios.get(`/api/bookings/email/${encodeURIComponent(searchEmail)}`)
      if (response.data.length === 0) {
        setEmailError('Bu e-posta adresine ait rezervasyon bulunamadı')
        onBookingsFound([])
      } else {
        onBookingsFound(response.data)
      }
    } catch (err) {
      setEmailError(err.response?.data?.message || 'Rezervasyonlar yüklenirken hata oluştu')
      onBookingsFound([])
    } finally {
      setEmailLoading(false)
    }
  }

  return (
    <form onSubmit={handleEmailSearch}>
      <div className="form-group">
        <label>E-posta Adresi</label>
        <input
          type="email"
          value={searchEmail}
          onChange={(e) => setSearchEmail(e.target.value)}
          placeholder="ornek@email.com"
          required
        />
        <small style={{ color: '#666', fontSize: '12px' }}>
          Rezervasyon yaparken kullandığınız e-posta adresini girin
        </small>
      </div>

      {emailError && <div className="error">{emailError}</div>}

      <button type="submit" className="btn btn-primary" disabled={emailLoading} style={{ width: '100%' }}>
        {emailLoading ? '📧 Aranıyor...' : '📧 E-posta ile Ara'}
      </button>
    </form>
  )
}

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

function GuestBooking({ user }) {
  const location = useLocation()
  const [searchParams] = useSearchParams()
  const [pnr, setPnr] = useState('')
  const [email, setEmail] = useState('')
  const [booking, setBooking] = useState(null)
  const [bookings, setBookings] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [showPaymentModal, setShowPaymentModal] = useState(false)
  const [paymentLoading, setPaymentLoading] = useState(false)
  const [paymentData, setPaymentData] = useState({
    cardNumber: '',
    expiryDate: '',
    cvv: '',
    cardHolder: ''
  })

  // URL state'inden ve query parametrelerinden PNR ve email'i al
  useEffect(() => {
    let pnrValue = ''
    let emailValue = ''

    // Önce location state'den kontrol et
    if (location.state?.pnr) {
      pnrValue = location.state.pnr
      setPnr(pnrValue)
    }
    if (location.state?.email) {
      emailValue = location.state.email
      setEmail(emailValue)
    }

    // Sonra URL query parametrelerinden kontrol et
    const urlPnr = searchParams.get('pnr')
    const urlEmail = searchParams.get('email')
    
    if (urlPnr && !pnrValue) {
      pnrValue = urlPnr
      setPnr(pnrValue)
    }
    if (urlEmail && !emailValue) {
      emailValue = urlEmail
      setEmail(emailValue)
    }

    // Eğer her ikisi de varsa otomatik sorgula
    if (pnrValue && emailValue) {
      handleAutoSearch(pnrValue, emailValue)
    }
  }, [location.state, searchParams])

  const handleAutoSearch = async (pnrValue, emailValue) => {
    setError('')
    setLoading(true)
    setBooking(null)

    try {
      const response = await axios.get(`/api/bookings/pnr/${pnrValue.toUpperCase()}?email=${encodeURIComponent(emailValue)}`)
      setBooking(response.data)
    } catch (err) {
      setError(err.response?.data?.message || 'Rezervasyon bulunamadı')
    } finally {
      setLoading(false)
    }
  }

  const handleSearch = async (e) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    setBooking(null)

    try {
      const response = await axios.get(`/api/bookings/pnr/${pnr.toUpperCase()}?email=${encodeURIComponent(email)}`)
      setBooking(response.data)
    } catch (err) {
      setError(err.response?.data?.message || 'Rezervasyon bulunamadı')
    } finally {
      setLoading(false)
    }
  }

  const handlePaymentClick = () => {
    setShowPaymentModal(true)
  }

  const handlePaymentSubmit = async (e) => {
    e.preventDefault()
    
    // Kart bilgilerini doğrula
    if (!paymentData.cardNumber || paymentData.cardNumber.replace(/\s/g, '').length !== 16) {
      alert('❌ Geçerli bir kart numarası girin (16 hane)')
      return
    }
    
    if (!paymentData.expiryDate || !/^\d{2}\/\d{2}$/.test(paymentData.expiryDate)) {
      alert('❌ Geçerli bir son kullanma tarihi girin (AA/YY)')
      return
    }
    
    if (!paymentData.cvv || paymentData.cvv.length !== 3) {
      alert('❌ Geçerli bir CVV girin (3 hane)')
      return
    }
    
    if (!paymentData.cardHolder.trim()) {
      alert('❌ Kart sahibinin adını girin')
      return
    }

    setPaymentLoading(true)

    try {
      // Simüle edilmiş ödeme işlemi (2 saniye bekle)
      await new Promise(resolve => setTimeout(resolve, 2000))
      
      await axios.post(`/api/bookings/pnr/${pnr.toUpperCase()}/pay?email=${encodeURIComponent(email)}`)
      
      // Rezervasyonu yeniden yükle
      const response = await axios.get(`/api/bookings/pnr/${pnr.toUpperCase()}?email=${encodeURIComponent(email)}`)
      setBooking(response.data)
      
      setShowPaymentModal(false)
      setPaymentData({ cardNumber: '', expiryDate: '', cvv: '', cardHolder: '' })
      
      // Başarı mesajı göster
      if (user) {
        alert('✅ Ödeme başarıyla tamamlandı!\n\n🎉 Biletiniz e-posta adresinize gönderilmiştir.')
      } else {
        alert('✅ Ödeme başarıyla tamamlandı!')
      }
      
      // Üyelik teklifi - sadece misafir kullanıcılar için
      if (!user) {
        const wantToRegister = confirm(
          '🎉 Ödemeniz tamamlandı!\n\n' +
          '💡 Üye olarak daha fazla avantajdan yararlanabilirsiniz:\n' +
          '• Tüm rezervasyonlarınızı tek yerden yönetin\n' +
          '• Hızlı rezervasyon yapın\n' +
          '• Özel kampanyalardan haberdar olun\n\n' +
          'Şimdi üye olmak ister misiniz?'
        )
        
        if (wantToRegister) {
          // Kayıt sayfasına yönlendir, otomatik kayıt yapma
          window.location.href = '/register?email=' + encodeURIComponent(email)
        }
      }
    } catch (err) {
      alert('❌ ' + (err.response?.data?.message || 'Ödeme yapılırken bir hata oluştu'))
    } finally {
      setPaymentLoading(false)
    }
  }

  const handlePaymentInputChange = (field, value) => {
    let formattedValue = value

    if (field === 'cardNumber') {
      // Sadece rakamları al ve 4'lü gruplar halinde formatla
      formattedValue = value.replace(/\D/g, '').replace(/(\d{4})(?=\d)/g, '$1 ').trim()
      if (formattedValue.length > 19) formattedValue = formattedValue.slice(0, 19) // 16 rakam + 3 boşluk
    } else if (field === 'expiryDate') {
      // AA/YY formatında
      formattedValue = value.replace(/\D/g, '').replace(/(\d{2})(\d)/, '$1/$2').slice(0, 5)
    } else if (field === 'cvv') {
      // Sadece 3 rakam
      formattedValue = value.replace(/\D/g, '').slice(0, 3)
    } else if (field === 'cardHolder') {
      // Sadece harfler ve boşluk
      formattedValue = value.replace(/[^a-zA-ZğüşıöçĞÜŞİÖÇ\s]/g, '').toUpperCase()
    }

    setPaymentData(prev => ({
      ...prev,
      [field]: formattedValue
    }))
  }

  return (
    <div className="container">
      <div className="card" style={{ maxWidth: '600px', margin: '40px auto' }}>
        <div style={{
          background: 'linear-gradient(135deg, #00bcd4 0%, #00acc1 100%)',
          color: 'white',
          padding: '40px',
          borderRadius: '24px',
          marginBottom: '40px',
          textAlign: 'center'
        }}>
          <h1 style={{ fontSize: '36px', marginBottom: '16px', fontWeight: '800' }}>
            BULUTBİLET<span style={{ color: '#00e5ff' }}>.COM</span> - Rezervasyon Sorgula
          </h1>
          <p style={{ fontSize: '16px' }}>
            {user 
              ? 'PNR numaranız ve e-posta adresinizle rezervasyonunuzu görüntüleyin ve ödeme yapın'
              : 'PNR numaranız ve e-posta adresinizle rezervasyonunuzu görüntüleyin'
            }
          </p>
        </div>

        <form onSubmit={handleSearch}>
          <div className="form-group">
            <label>PNR Numarası</label>
            <input
              type="text"
              value={pnr}
              onChange={(e) => setPnr(e.target.value.toUpperCase())}
              placeholder="ABC123"
              required
              maxLength="6"
              style={{ textTransform: 'uppercase', fontSize: '20px', fontWeight: 'bold', textAlign: 'center' }}
            />
            <small style={{ color: '#666', fontSize: '12px' }}>
              6 karakterli PNR kodunuzu girin
            </small>
          </div>

          <div className="form-group">
            <label>E-posta Adresi</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="ornek@email.com"
              required
            />
          </div>

          {error && <div className="error">{error}</div>}

          <button type="submit" className="btn btn-primary" disabled={loading} style={{ width: '100%' }}>
            {loading ? '🔍 Aranıyor...' : '🔍 Rezervasyonu Bul'}
          </button>
        </form>
      </div>

      {/* E-posta ile Sorgulama */}
      <div className="card" style={{ maxWidth: '600px', margin: '20px auto' }}>
        <div style={{
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          color: 'white',
          padding: '24px',
          borderRadius: '16px',
          marginBottom: '24px',
          textAlign: 'center'
        }}>
          <h2 style={{ fontSize: '24px', marginBottom: '8px', fontWeight: '700' }}>
            📧 E-posta ile Sorgulama
          </h2>
          <p style={{ fontSize: '14px', opacity: '0.9' }}>
            PNR numaranızı unuttuysanız, sadece e-posta adresinizle tüm rezervasyonlarınızı görüntüleyebilirsiniz
          </p>
        </div>

        <EmailSearchForm onBookingsFound={(bookings) => setBookings(bookings)} />
      </div>

      {booking && (
        <div className="card" style={{ maxWidth: '800px', margin: '0 auto' }}>
          <div style={{
            background: 'linear-gradient(135deg, #4caf50 0%, #388e3c 100%)',
            color: 'white',
            padding: '24px',
            borderRadius: '20px',
            marginBottom: '32px'
          }}>
            <h2 style={{ fontSize: '32px', marginBottom: '12px', fontWeight: '800' }}>✅ Rezervasyon Bulundu!</h2>
            <p style={{ fontSize: '20px', fontWeight: '700' }}>PNR: {booking.bookingReference}</p>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '16px', marginBottom: '24px' }}>
            <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
              <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Uçuş</div>
              <div style={{ fontSize: '18px', fontWeight: 'bold' }}>{booking.flight.flightNumber}</div>
              <div style={{ fontSize: '14px', color: '#666' }}>{booking.flight.airline}</div>
            </div>

            <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
              <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Rota</div>
              <div style={{ fontSize: '16px', fontWeight: 'bold' }}>
                {booking.flight.departureCity} ({airportMapping[booking.flight.departureCity]?.code || 'N/A'}) → {booking.flight.arrivalCity} ({airportMapping[booking.flight.arrivalCity]?.code || 'N/A'})
              </div>
              <div style={{ fontSize: '12px', color: '#888', marginTop: '2px' }}>
                {airportMapping[booking.flight.departureCity]?.name || booking.flight.departureCity} → {airportMapping[booking.flight.arrivalCity]?.name || booking.flight.arrivalCity}
              </div>
            </div>

            <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
              <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Kalkış</div>
              <div style={{ fontSize: '14px', fontWeight: 'bold' }}>
                {new Date(booking.flight.departureTime).toLocaleString('tr-TR')}
              </div>
            </div>

            <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
              <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Yolcu Sayısı</div>
              <div style={{ fontSize: '24px', fontWeight: 'bold' }}>{booking.passengerCount}</div>
            </div>

            <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
              <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Toplam Tutar</div>
              <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#667eea' }}>₺{booking.totalPrice}</div>
            </div>

            <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
              <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Rezervasyon Durumu</div>
              <div style={{ fontSize: '16px', fontWeight: 'bold', color: booking.isPaid ? '#37b24d' : booking.status === 'Cancelled' ? '#f03e3e' : '#667eea' }}>
                {getStatusText(booking.status, booking.isPaid)}
              </div>
            </div>
          </div>

          {booking.passengers && booking.passengers.length > 0 && (
            <div style={{ marginBottom: '24px' }}>
              <h3 style={{ marginBottom: '16px' }}>👥 Yolcular</h3>
              <div style={{ display: 'grid', gap: '12px' }}>
                {booking.passengers.map((passenger, index) => (
                  <div key={index} style={{
                    padding: '16px',
                    background: 'linear-gradient(135deg, #f8f9ff 0%, #e8ecff 100%)',
                    borderRadius: '12px',
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    flexWrap: 'wrap',
                    gap: '12px'
                  }}>
                    <div>
                      <div style={{ fontSize: '18px', fontWeight: 'bold' }}>
                        {passenger.firstName} {passenger.lastName}
                      </div>
                      <div style={{ fontSize: '14px', color: '#666' }}>
                        {passenger.gender} • {new Date(passenger.dateOfBirth).toLocaleDateString('tr-TR')}
                      </div>
                      {passenger.seatNumber && (
                        <div style={{ 
                          fontSize: '14px', 
                          marginTop: '8px',
                          background: 'linear-gradient(135deg, #667eea, #764ba2)',
                          color: 'white',
                          padding: '4px 12px',
                          borderRadius: '12px',
                          display: 'inline-block'
                        }}>
                          🪑 Koltuk: {passenger.seatNumber} ({passenger.seatType === 'Window' ? '🪟 Cam Kenarı' : passenger.seatType === 'Aisle' ? '🚶 Koridor' : '💺 Orta'})
                        </div>
                      )}
                    </div>
                    <div style={{ fontSize: '14px', color: '#666' }}>
                      TC: {passenger.identityNumber}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {!booking.isPaid && booking.status === 'Confirmed' && (
            <div style={{
              background: 'linear-gradient(135deg, #ff9800 0%, #f57c00 100%)',
              color: 'white',
              padding: '32px',
              borderRadius: '20px',
              textAlign: 'center'
            }}>
              <h3 style={{ fontSize: '28px', marginBottom: '20px', fontWeight: '700' }}>💳 Ödeme Yapın</h3>
              <p style={{ fontSize: '18px', marginBottom: '24px', fontWeight: '500' }}>
                Rezervasyonunuzu tamamlamak için ödeme yapmanız gerekmektedir
              </p>
              <button 
                className="btn btn-success" 
                onClick={handlePaymentClick}
                style={{ fontSize: '18px', padding: '16px 48px' }}
              >
                💳 ₺{booking.totalPrice} Öde
              </button>
            </div>
          )}

          {booking.isPaid && (
            <div style={{
              background: 'linear-gradient(135deg, #4caf50 0%, #388e3c 100%)',
              color: 'white',
              padding: '32px',
              borderRadius: '20px',
              textAlign: 'center'
            }}>
              <h3 style={{ fontSize: '28px', marginBottom: '12px', fontWeight: '700' }}>🎉 Ödeme Tamamlandı!</h3>
              <p style={{ fontSize: '18px', fontWeight: '500' }}>
                Biletiniz e-posta adresinize gönderilmiştir
              </p>
            </div>
          )}
        </div>
      )}

      {/* E-posta ile Bulunan Rezervasyonlar */}
      {bookings && bookings.length > 0 && (
        <div className="card" style={{ maxWidth: '1000px', margin: '20px auto' }}>
          <div style={{
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            color: 'white',
            padding: '24px',
            borderRadius: '20px',
            marginBottom: '32px',
            textAlign: 'center'
          }}>
            <h2 style={{ fontSize: '28px', marginBottom: '12px', fontWeight: '800' }}>📧 E-posta Rezervasyonları</h2>
            <p style={{ fontSize: '16px', fontWeight: '500' }}>{bookings.length} rezervasyon bulundu</p>
          </div>

          <div style={{ display: 'grid', gap: '20px' }}>
            {bookings.map((emailBooking, index) => (
              <div key={index} style={{
                border: '2px solid #e0e0e0',
                borderRadius: '16px',
                padding: '24px',
                background: 'linear-gradient(135deg, #f8f9ff 0%, #ffffff 100%)'
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px', flexWrap: 'wrap', gap: '12px' }}>
                  <div>
                    <h3 style={{ fontSize: '24px', fontWeight: 'bold', color: '#333', marginBottom: '4px' }}>
                      PNR: {emailBooking.bookingReference}
                    </h3>
                    <div style={{ fontSize: '16px', color: '#666' }}>
                      {emailBooking.flight.flightNumber} - {emailBooking.flight.airline}
                    </div>
                  </div>
                  <div style={{
                    padding: '8px 16px',
                    borderRadius: '20px',
                    fontSize: '14px',
                    fontWeight: 'bold',
                    background: emailBooking.isPaid ? '#4caf50' : emailBooking.status === 'Cancelled' ? '#f44336' : '#ff9800',
                    color: 'white'
                  }}>
                    {getStatusText(emailBooking.status, emailBooking.isPaid)}
                  </div>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '16px', marginBottom: '16px' }}>
                  <div style={{ padding: '12px', background: 'rgba(255,255,255,0.7)', borderRadius: '8px' }}>
                    <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Rota</div>
                    <div style={{ fontSize: '14px', fontWeight: 'bold' }}>
                      {emailBooking.flight.departureCity} → {emailBooking.flight.arrivalCity}
                    </div>
                  </div>
                  <div style={{ padding: '12px', background: 'rgba(255,255,255,0.7)', borderRadius: '8px' }}>
                    <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Kalkış</div>
                    <div style={{ fontSize: '14px', fontWeight: 'bold' }}>
                      {new Date(emailBooking.flight.departureTime).toLocaleDateString('tr-TR')}
                    </div>
                  </div>
                  <div style={{ padding: '12px', background: 'rgba(255,255,255,0.7)', borderRadius: '8px' }}>
                    <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Yolcu</div>
                    <div style={{ fontSize: '14px', fontWeight: 'bold' }}>{emailBooking.passengerCount} kişi</div>
                  </div>
                  <div style={{ padding: '12px', background: 'rgba(255,255,255,0.7)', borderRadius: '8px' }}>
                    <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Tutar</div>
                    <div style={{ fontSize: '16px', fontWeight: 'bold', color: '#667eea' }}>₺{emailBooking.totalPrice}</div>
                  </div>
                </div>

                <div style={{ display: 'flex', gap: '12px', justifyContent: 'center', flexWrap: 'wrap' }}>
                  <button
                    className="btn btn-primary"
                    onClick={() => {
                      // PNR detayına git
                      window.location.href = `/guest-booking?pnr=${emailBooking.bookingReference}&email=${encodeURIComponent(emailBooking.email)}`
                    }}
                    style={{ fontSize: '14px', padding: '8px 16px' }}
                  >
                    🔍 Detay Görüntüle
                  </button>
                  {!emailBooking.isPaid && emailBooking.status === 'Confirmed' && (
                    <button
                      className="btn btn-success"
                      onClick={() => {
                        // Ödeme sayfasına git
                        window.location.href = `/guest-booking?pnr=${emailBooking.bookingReference}&email=${encodeURIComponent(emailBooking.email)}`
                      }}
                      style={{ fontSize: '14px', padding: '8px 16px' }}
                    >
                      💳 Ödeme Yap
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Ödeme Modal */}
      {showPaymentModal && (
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
          zIndex: 9999,
          padding: '20px',
          boxSizing: 'border-box'
        }}>
          <div style={{
            background: 'white',
            borderRadius: '20px',
            padding: '30px',
            maxWidth: '500px',
            width: '100%',
            maxHeight: '90vh',
            overflow: 'auto'
          }}>
            <div style={{
              background: 'linear-gradient(135deg, #00bcd4 0%, #00acc1 100%)',
              color: 'white',
              padding: '24px',
              borderRadius: '20px',
              marginBottom: '32px',
              textAlign: 'center'
            }}>
              <h2 style={{ fontSize: '28px', marginBottom: '12px', fontWeight: '700' }}>💳 Ödeme Bilgileri</h2>
              <p style={{ fontSize: '18px', fontWeight: 'bold' }}>Toplam: ₺{booking?.totalPrice}</p>
            </div>

            <form onSubmit={handlePaymentSubmit}>
              <div className="form-group">
                <label>Kart Numarası</label>
                <input
                  type="text"
                  value={paymentData.cardNumber}
                  onChange={(e) => handlePaymentInputChange('cardNumber', e.target.value)}
                  placeholder="1234 5678 9012 3456"
                  required
                  style={{ 
                    fontSize: '18px', 
                    fontFamily: 'monospace',
                    letterSpacing: '2px',
                    textAlign: 'center'
                  }}
                />
                <small style={{ color: '#666', fontSize: '12px' }}>
                  16 haneli kart numaranızı girin
                </small>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                <div className="form-group">
                  <label>Son Kullanma Tarihi</label>
                  <input
                    type="text"
                    value={paymentData.expiryDate}
                    onChange={(e) => handlePaymentInputChange('expiryDate', e.target.value)}
                    placeholder="MM/YY"
                    required
                    style={{ 
                      fontSize: '18px', 
                      fontFamily: 'monospace',
                      textAlign: 'center'
                    }}
                  />
                </div>

                <div className="form-group">
                  <label>CVV</label>
                  <input
                    type="text"
                    value={paymentData.cvv}
                    onChange={(e) => handlePaymentInputChange('cvv', e.target.value)}
                    placeholder="123"
                    required
                    style={{ 
                      fontSize: '18px', 
                      fontFamily: 'monospace',
                      textAlign: 'center'
                    }}
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Kart Sahibinin Adı</label>
                <input
                  type="text"
                  value={paymentData.cardHolder}
                  onChange={(e) => handlePaymentInputChange('cardHolder', e.target.value)}
                  placeholder="AHMET YILMAZ"
                  required
                  style={{ 
                    fontSize: '16px',
                    textTransform: 'uppercase'
                  }}
                />
                <small style={{ color: '#666', fontSize: '12px' }}>
                  Kartınızda yazıldığı gibi girin
                </small>
              </div>

              <div style={{
                background: '#f8f9ff',
                padding: '16px',
                borderRadius: '12px',
                marginBottom: '20px',
                border: '1px solid #e0e0e0'
              }}>
                <h4 style={{ marginBottom: '12px', color: '#333' }}>🔒 Güvenli Ödeme</h4>
                <p style={{ fontSize: '14px', color: '#666', marginBottom: '8px' }}>
                  • Kart bilgileriniz SSL ile şifrelenir
                </p>
                <p style={{ fontSize: '14px', color: '#666', marginBottom: '8px' }}>
                  • 3D Secure ile güvenli ödeme
                </p>
                <p style={{ fontSize: '14px', color: '#666' }}>
                  • Kart bilgileriniz saklanmaz
                </p>
              </div>

              <div style={{ display: 'flex', gap: '12px' }}>
                <button 
                  type="submit" 
                  className="btn btn-success" 
                  disabled={paymentLoading}
                  style={{ flex: 1, fontSize: '16px', padding: '14px' }}
                >
                  {paymentLoading ? '💳 Ödeme Yapılıyor...' : `💳 ₺${booking?.totalPrice} Öde`}
                </button>
                <button 
                  type="button" 
                  className="btn btn-secondary"
                  onClick={() => setShowPaymentModal(false)}
                  disabled={paymentLoading}
                  style={{ fontSize: '16px', padding: '14px' }}
                >
                  İptal
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}

export default GuestBooking
