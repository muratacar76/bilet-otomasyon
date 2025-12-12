import { useState, useEffect } from 'react'
import axios from 'axios'

// Rezervasyon durumu çevirisi
const getStatusText = (status, isPaid) => {
  if (status === 'Cancelled') return 'İptal Edildi'
  if (status === 'Confirmed' && isPaid) return 'Ödendi'
  if (status === 'Confirmed' && !isPaid) return 'Onaylandı'
  return status
}

function Bookings() {
  const [bookings, setBookings] = useState([])
  const [loading, setLoading] = useState(true)
  const [showPaymentModal, setShowPaymentModal] = useState(false)
  const [selectedBooking, setSelectedBooking] = useState(null)
  const [paymentLoading, setPaymentLoading] = useState(false)
  const [paymentData, setPaymentData] = useState({
    cardNumber: '',
    expiryDate: '',
    cvv: '',
    cardHolder: ''
  })

  useEffect(() => {
    fetchBookings()
  }, [])

  const fetchBookings = async () => {
    try {
      const token = localStorage.getItem('token')
      const response = await axios.get('/api/bookings', {
        headers: { Authorization: `Bearer ${token}` }
      })
      setBookings(response.data)
    } catch (error) {
      console.error('Rezervasyonlar yüklenemedi:', error)
    } finally {
      setLoading(false)
    }
  }

  const handlePaymentClick = (booking) => {
    setSelectedBooking(booking)
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
      
      const token = localStorage.getItem('token')
      await axios.post(`/api/bookings/${selectedBooking.id}/pay`, {}, {
        headers: { Authorization: `Bearer ${token}` }
      })
      
      setShowPaymentModal(false)
      setSelectedBooking(null)
      setPaymentData({ cardNumber: '', expiryDate: '', cvv: '', cardHolder: '' })
      
      alert('✅ Ödeme başarıyla tamamlandı!')
      
      // Misafir kullanıcıysa üyelik teklif et
      const userData = localStorage.getItem('user')
      if (userData) {
        const user = JSON.parse(userData)
        if (user.email && !user.firstName) {
          const wantToRegister = confirm(
            '🎉 Ödemeniz tamamlandı!\n\n' +
            '💡 Üye olarak daha fazla avantajdan yararlanabilirsiniz:\n' +
            '• Tüm rezervasyonlarınızı tek yerden yönetin\n' +
            '• Hızlı rezervasyon yapın\n' +
            '• Özel kampanyalardan haberdar olun\n\n' +
            'Şimdi üye olmak ister misiniz?'
          )
          
          if (wantToRegister) {
            window.location.href = '/register?from=payment&email=' + encodeURIComponent(user.email)
            return
          }
        }
      }
      
      fetchBookings()
    } catch (error) {
      const errorMsg = error.response?.data?.message || 'Ödeme yapılırken bir hata oluştu'
      alert('❌ ' + errorMsg)
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

  const handleCancel = async (bookingId) => {
    if (!confirm('⚠️ Rezervasyonu iptal etmek istediğinizden emin misiniz?')) return

    try {
      const token = localStorage.getItem('token')
      await axios.delete(`/api/bookings/${bookingId}`, {
        headers: { Authorization: `Bearer ${token}` }
      })
      alert('✅ Rezervasyon başarıyla iptal edildi')
      fetchBookings()
    } catch (error) {
      const errorMsg = error.response?.data?.message || 'İptal işlemi sırasında bir hata oluştu'
      alert('❌ ' + errorMsg)
    }
  }

  if (loading) return <div className="loading">Yükleniyor...</div>

  return (
    <div className="container">
      <h2 style={{ color: 'white', marginBottom: '32px', fontSize: '32px', fontWeight: '800' }}>
        BULUTBİLET<span style={{ color: '#00e5ff' }}>.COM</span> - Rezervasyonlarım
      </h2>
      
      {bookings.length === 0 ? (
        <div className="card">
          <p>Henüz rezervasyonunuz bulunmamaktadır.</p>
        </div>
      ) : (
        <div className="grid">
          {bookings.map(booking => (
            <div key={booking.id} className="booking-card">
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '16px' }}>
                <h3>{booking.flight.departureCity} → {booking.flight.arrivalCity}</h3>
                <span className={`booking-status status-${booking.status.toLowerCase()}`}>
                  {getStatusText(booking.status, booking.isPaid)}
                </span>
              </div>
              
              <div style={{ marginBottom: '12px' }}>
                <p><strong>Rezervasyon No:</strong> {booking.bookingReference}</p>
                <p><strong>Uçuş No:</strong> {booking.flight.flightNumber}</p>
                <p><strong>Havayolu:</strong> {booking.flight.airline}</p>
                <p><strong>Kalkış:</strong> {new Date(booking.flight.departureTime).toLocaleString('tr-TR')}</p>
                <p><strong>Yolcu Sayısı:</strong> {booking.passengerCount}</p>
                <p><strong>Toplam Fiyat:</strong> ₺{booking.totalPrice}</p>
                <p><strong>Ödeme Durumu:</strong> {booking.isPaid ? '✅ Ödendi' : '❌ Ödenmedi'}</p>
              </div>

              {booking.passengers && booking.passengers.length > 0 && (
                <div style={{ marginBottom: '12px' }}>
                  <strong>Yolcular:</strong>
                  <ul style={{ marginTop: '8px', paddingLeft: '20px' }}>
                    {booking.passengers.map((passenger, index) => (
                      <li key={index}>
                        {passenger.firstName} {passenger.lastName} ({passenger.gender})
                      </li>
                    ))}
                  </ul>
                </div>
              )}

              <div style={{ display: 'flex', gap: '10px', marginTop: '16px' }}>
                {!booking.isPaid && booking.status === 'Confirmed' && (
                  <button 
                    className="btn btn-success" 
                    onClick={() => handlePaymentClick(booking)}
                  >
                    💳 Ödeme Yap
                  </button>
                )}
                {booking.status === 'Confirmed' && (
                  <button 
                    className="btn btn-danger" 
                    onClick={() => handleCancel(booking.id)}
                  >
                    İptal Et
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Ödeme Modal */}
      {showPaymentModal && selectedBooking && (
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
              <p style={{ fontSize: '16px', marginBottom: '8px' }}>
                {selectedBooking.flight.departureCity} → {selectedBooking.flight.arrivalCity}
              </p>
              <p style={{ fontSize: '18px', fontWeight: 'bold' }}>Toplam: ₺{selectedBooking.totalPrice}</p>
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
                  {paymentLoading ? '💳 Ödeme Yapılıyor...' : `💳 ₺${selectedBooking.totalPrice} Öde`}
                </button>
                <button 
                  type="button" 
                  className="btn btn-secondary"
                  onClick={() => {
                    setShowPaymentModal(false)
                    setSelectedBooking(null)
                    setPaymentData({ cardNumber: '', expiryDate: '', cvv: '', cardHolder: '' })
                  }}
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

export default Bookings
