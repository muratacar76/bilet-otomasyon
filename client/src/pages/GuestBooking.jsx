import { useState, useEffect } from 'react'
import { useLocation } from 'react-router-dom'
import axios from 'axios'

function GuestBooking() {
  const location = useLocation()
  const [pnr, setPnr] = useState('')
  const [email, setEmail] = useState('')
  const [booking, setBooking] = useState(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  // URL state'inden PNR ve email'i al
  useEffect(() => {
    if (location.state?.pnr) {
      setPnr(location.state.pnr)
    }
    if (location.state?.email) {
      setEmail(location.state.email)
    }
    // Eğer her ikisi de varsa otomatik sorgula
    if (location.state?.pnr && location.state?.email) {
      handleAutoSearch(location.state.pnr, location.state.email)
    }
  }, [location.state])

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

  const handlePayment = async () => {
    if (!confirm('Ödemeyi onaylıyor musunuz?')) return

    try {
      await axios.post(`/api/bookings/pnr/${pnr.toUpperCase()}/pay?email=${encodeURIComponent(email)}`)
      alert('✅ Ödeme başarıyla tamamlandı!')
      
      // Rezervasyonu yeniden yükle
      const response = await axios.get(`/api/bookings/pnr/${pnr.toUpperCase()}?email=${encodeURIComponent(email)}`)
      setBooking(response.data)
      
      // Üyelik teklifi
      const wantToRegister = confirm(
        '🎉 Ödemeniz tamamlandı!\n\n' +
        '💡 Üye olarak daha fazla avantajdan yararlanabilirsiniz:\n' +
        '• Tüm rezervasyonlarınızı tek yerden yönetin\n' +
        '• Hızlı rezervasyon yapın\n' +
        '• Özel kampanyalardan haberdar olun\n\n' +
        'Şimdi üye olmak ister misiniz?'
      )
      
      if (wantToRegister) {
        window.location.href = '/register?email=' + encodeURIComponent(email)
      }
    } catch (err) {
      alert('❌ ' + (err.response?.data?.message || 'Ödeme yapılırken bir hata oluştu'))
    }
  }

  return (
    <div className="container">
      <div className="card" style={{ maxWidth: '600px', margin: '40px auto' }}>
        <div style={{
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          color: 'white',
          padding: '30px',
          borderRadius: '20px',
          marginBottom: '30px',
          textAlign: 'center'
        }}>
          <h1 style={{ fontSize: '32px', marginBottom: '12px' }}>🎫 Rezervasyon Sorgula</h1>
          <p style={{ fontSize: '16px' }}>PNR numaranız ve e-posta adresinizle rezervasyonunuzu görüntüleyin</p>
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

      {booking && (
        <div className="card" style={{ maxWidth: '800px', margin: '0 auto' }}>
          <div style={{
            background: 'linear-gradient(135deg, #51cf66 0%, #37b24d 100%)',
            color: 'white',
            padding: '20px',
            borderRadius: '15px',
            marginBottom: '24px'
          }}>
            <h2 style={{ fontSize: '28px', marginBottom: '8px' }}>✅ Rezervasyon Bulundu!</h2>
            <p style={{ fontSize: '18px', fontWeight: 'bold' }}>PNR: {booking.bookingReference}</p>
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
                {booking.flight.departureCity} → {booking.flight.arrivalCity}
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
              <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Ödeme Durumu</div>
              <div style={{ fontSize: '16px', fontWeight: 'bold', color: booking.isPaid ? '#37b24d' : '#f03e3e' }}>
                {booking.isPaid ? '✅ Ödendi' : '❌ Ödenmedi'}
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
              background: 'linear-gradient(135deg, #ffd43b 0%, #fab005 100%)',
              padding: '24px',
              borderRadius: '15px',
              textAlign: 'center'
            }}>
              <h3 style={{ fontSize: '24px', marginBottom: '16px' }}>💳 Ödeme Yapın</h3>
              <p style={{ fontSize: '16px', marginBottom: '20px' }}>
                Rezervasyonunuzu tamamlamak için ödeme yapmanız gerekmektedir
              </p>
              <button 
                className="btn btn-success" 
                onClick={handlePayment}
                style={{ fontSize: '18px', padding: '16px 48px' }}
              >
                💳 ₺{booking.totalPrice} Öde
              </button>
            </div>
          )}

          {booking.isPaid && (
            <div style={{
              background: 'linear-gradient(135deg, #51cf66 0%, #37b24d 100%)',
              color: 'white',
              padding: '24px',
              borderRadius: '15px',
              textAlign: 'center'
            }}>
              <h3 style={{ fontSize: '24px', marginBottom: '8px' }}>🎉 Ödeme Tamamlandı!</h3>
              <p style={{ fontSize: '16px' }}>
                Biletiniz e-posta adresinize gönderilmiştir
              </p>
            </div>
          )}
        </div>
      )}
    </div>
  )
}

export default GuestBooking
