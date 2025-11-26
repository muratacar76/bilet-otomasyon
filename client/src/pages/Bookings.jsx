import { useState, useEffect } from 'react'
import axios from 'axios'

function Bookings() {
  const [bookings, setBookings] = useState([])
  const [loading, setLoading] = useState(true)

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

  const handlePayment = async (bookingId) => {
    try {
      const token = localStorage.getItem('token')
      await axios.post(`/api/bookings/${bookingId}/pay`, {}, {
        headers: { Authorization: `Bearer ${token}` }
      })
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
    }
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
      <h2 style={{ color: 'white', marginBottom: '24px' }}>Rezervasyonlarım</h2>
      
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
                  {booking.status}
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
                    onClick={() => handlePayment(booking.id)}
                  >
                    Ödeme Yap
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
    </div>
  )
}

export default Bookings
