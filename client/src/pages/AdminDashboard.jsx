import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import axios from 'axios'

// Rezervasyon durumu çevirisi
const getStatusText = (status, isPaid) => {
  if (status === 'Cancelled') return 'İptal Edildi'
  if (status === 'Confirmed' && isPaid) return 'Ödendi'
  if (status === 'Confirmed' && !isPaid) return 'Onaylandı'
  return status
}

function AdminDashboard() {
  const [flights, setFlights] = useState([])
  const [bookings, setBookings] = useState([])
  const [showFlightForm, setShowFlightForm] = useState(false)
  const [editingFlight, setEditingFlight] = useState(null)
  const [flightForm, setFlightForm] = useState({
    flightNumber: '',
    airline: '',
    departureCity: '',
    arrivalCity: '',
    departureTime: '',
    arrivalTime: '',
    price: '',
    totalSeats: ''
  })

  useEffect(() => {
    fetchFlights()
    fetchAllBookings()
  }, [])

  const fetchFlights = async () => {
    try {
      const response = await axios.get('/api/flights')
      setFlights(response.data)
    } catch (error) {
      console.error('Uçuşlar yüklenemedi:', error)
    }
  }

  const fetchAllBookings = async () => {
    try {
      const token = localStorage.getItem('token')
      const response = await axios.get('/api/bookings/all', {
        headers: { Authorization: `Bearer ${token}` }
      })
      setBookings(response.data)
    } catch (error) {
      console.error('Rezervasyonlar yüklenemedi:', error)
    }
  }

  const handleFlightSubmit = async (e) => {
    e.preventDefault()
    const token = localStorage.getItem('token')

    try {
      if (editingFlight) {
        await axios.put(`/api/flights/${editingFlight.id}`, flightForm, {
          headers: { Authorization: `Bearer ${token}` }
        })
        alert('✅ Uçuş başarıyla güncellendi')
      } else {
        await axios.post('/api/flights', flightForm, {
          headers: { Authorization: `Bearer ${token}` }
        })
        alert('✅ Uçuş başarıyla eklendi')
      }
      
      setShowFlightForm(false)
      setEditingFlight(null)
      setFlightForm({
        flightNumber: '',
        airline: '',
        departureCity: '',
        arrivalCity: '',
        departureTime: '',
        arrivalTime: '',
        price: '',
        totalSeats: ''
      })
      fetchFlights()
    } catch (error) {
      const errorMsg = error.response?.data?.message || 'İşlem sırasında bir hata oluştu'
      alert('❌ ' + errorMsg)
    }
  }

  const handleEdit = (flight) => {
    setEditingFlight(flight)
    setFlightForm({
      flightNumber: flight.flightNumber,
      airline: flight.airline,
      departureCity: flight.departureCity,
      arrivalCity: flight.arrivalCity,
      departureTime: flight.departureTime.slice(0, 16),
      arrivalTime: flight.arrivalTime.slice(0, 16),
      price: flight.price,
      totalSeats: flight.totalSeats
    })
    setShowFlightForm(true)
  }

  const handleDelete = async (flightId) => {
    if (!confirm('⚠️ Uçuşu silmek istediğinizden emin misiniz?')) return

    try {
      const token = localStorage.getItem('token')
      await axios.delete(`/api/flights/${flightId}`, {
        headers: { Authorization: `Bearer ${token}` }
      })
      alert('✅ Uçuş başarıyla silindi')
      fetchFlights()
    } catch (error) {
      const errorMsg = error.response?.data?.message || 'Silme işlemi sırasında bir hata oluştu'
      alert('❌ ' + errorMsg)
    }
  }

  const handleCancelBooking = async (bookingId) => {
    if (!confirm('⚠️ Rezervasyonu iptal etmek istediğinizden emin misiniz?')) return

    try {
      const token = localStorage.getItem('token')
      await axios.delete(`/api/bookings/${bookingId}`, {
        headers: { Authorization: `Bearer ${token}` }
      })
      alert('✅ Rezervasyon başarıyla iptal edildi')
      fetchAllBookings()
      fetchFlights()
    } catch (error) {
      const errorMsg = error.response?.data?.message || 'İptal işlemi sırasında bir hata oluştu'
      alert('❌ ' + errorMsg)
    }
  }

  const handleDeleteAllBookings = async () => {
    const confirmMessage = `⚠️ DİKKAT: Tüm rezervasyonları silmek istediğinizden emin misiniz?\n\n` +
                          `• Toplam ${bookings.length} rezervasyon silinecek\n` +
                          `• Bu işlem geri alınamaz\n` +
                          `• Uçuşlardaki koltuk sayıları sıfırlanacak\n\n` +
                          `Devam etmek için "EVET" yazın:`

    const userInput = prompt(confirmMessage)
    
    if (userInput !== 'EVET') {
      alert('❌ İşlem iptal edildi')
      return
    }

    try {
      const token = localStorage.getItem('token')
      
      const response = await axios.delete('/api/bookings/all', {
        headers: { Authorization: `Bearer ${token}` }
      })
      
      const { deletedCount, deletedPassengers } = response.data
      alert(`✅ Toplu silme tamamlandı!\n\n• ${deletedCount} rezervasyon silindi\n• ${deletedPassengers} yolcu kaydı silindi\n• Uçuş koltuk sayıları güncellendi`)
      
      fetchAllBookings()
      fetchFlights()
    } catch (error) {
      const errorMsg = error.response?.data?.message || 'Toplu silme işlemi sırasında bir hata oluştu'
      alert('❌ ' + errorMsg)
    }
  }

  return (
    <div className="container">
      <h2 style={{ color: 'white', marginBottom: '32px', fontSize: '32px', fontWeight: '800' }}>
        BULUTBİLET<span style={{ color: '#00e5ff' }}>.COM</span> - Admin Paneli
      </h2>

      {/* Admin Menü */}
      <div style={{ 
        display: 'flex', 
        gap: '16px', 
        marginBottom: '32px', 
        flexWrap: 'wrap',
        justifyContent: 'center'
      }}>
        <Link 
          to="/admin" 
          style={{ 
            padding: '12px 24px', 
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', 
            color: 'white', 
            textDecoration: 'none', 
            borderRadius: '12px',
            fontWeight: 'bold',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}
        >
          ✈️ Uçuş & Rezervasyon Yönetimi
        </Link>
        <Link 
          to="/admin/users" 
          style={{ 
            padding: '12px 24px', 
            background: 'linear-gradient(135deg, #4caf50 0%, #388e3c 100%)', 
            color: 'white', 
            textDecoration: 'none', 
            borderRadius: '12px',
            fontWeight: 'bold',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}
        >
          👥 Kullanıcı Yönetimi
        </Link>
      </div>

      <div className="card">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h3>Uçuş Yönetimi</h3>
          <button 
            className="btn btn-primary" 
            onClick={() => {
              setShowFlightForm(!showFlightForm)
              setEditingFlight(null)
              setFlightForm({
                flightNumber: '',
                airline: '',
                departureCity: '',
                arrivalCity: '',
                departureTime: '',
                arrivalTime: '',
                price: '',
                totalSeats: ''
              })
            }}
          >
            {showFlightForm ? 'İptal' : 'Yeni Uçuş Ekle'}
          </button>
        </div>

        {showFlightForm && (
          <form onSubmit={handleFlightSubmit} style={{ marginBottom: '24px', padding: '20px', background: '#f8f9fa', borderRadius: '8px' }}>
            <h4>{editingFlight ? 'Uçuş Düzenle' : 'Yeni Uçuş'}</h4>
            <div className="form-row">
              <input
                type="text"
                placeholder="Uçuş No"
                value={flightForm.flightNumber}
                onChange={(e) => setFlightForm({...flightForm, flightNumber: e.target.value})}
                required
              />
              <input
                type="text"
                placeholder="Havayolu"
                value={flightForm.airline}
                onChange={(e) => setFlightForm({...flightForm, airline: e.target.value})}
                required
              />
            </div>
            <div className="form-row">
              <input
                type="text"
                placeholder="Kalkış Şehri"
                value={flightForm.departureCity}
                onChange={(e) => setFlightForm({...flightForm, departureCity: e.target.value})}
                required
              />
              <input
                type="text"
                placeholder="Varış Şehri"
                value={flightForm.arrivalCity}
                onChange={(e) => setFlightForm({...flightForm, arrivalCity: e.target.value})}
                required
              />
            </div>
            <div className="form-row">
              <input
                type="datetime-local"
                value={flightForm.departureTime}
                onChange={(e) => setFlightForm({...flightForm, departureTime: e.target.value})}
                required
              />
              <input
                type="datetime-local"
                value={flightForm.arrivalTime}
                onChange={(e) => setFlightForm({...flightForm, arrivalTime: e.target.value})}
                required
              />
            </div>
            <div className="form-row">
              <input
                type="number"
                placeholder="Fiyat"
                value={flightForm.price}
                onChange={(e) => setFlightForm({...flightForm, price: e.target.value})}
                required
              />
              <input
                type="number"
                placeholder="Toplam Koltuk"
                value={flightForm.totalSeats}
                onChange={(e) => setFlightForm({...flightForm, totalSeats: e.target.value})}
                required
              />
            </div>
            <button type="submit" className="btn btn-success">
              {editingFlight ? 'Güncelle' : 'Ekle'}
            </button>
          </form>
        )}

        <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ background: '#f8f9fa' }}>
                <th style={{ padding: '12px', textAlign: 'left' }}>Uçuş No</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>Havayolu</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>Rota</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>Kalkış</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>Fiyat</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>Koltuk</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>İşlemler</th>
              </tr>
            </thead>
            <tbody>
              {flights.map(flight => (
                <tr key={flight.id} style={{ borderBottom: '1px solid #e0e0e0' }}>
                  <td style={{ padding: '12px' }}>{flight.flightNumber}</td>
                  <td style={{ padding: '12px' }}>{flight.airline}</td>
                  <td style={{ padding: '12px' }}>{flight.departureCity} → {flight.arrivalCity}</td>
                  <td style={{ padding: '12px' }}>{new Date(flight.departureTime).toLocaleString('tr-TR')}</td>
                  <td style={{ padding: '12px' }}>₺{flight.price}</td>
                  <td style={{ padding: '12px' }}>{flight.availableSeats}/{flight.totalSeats}</td>
                  <td style={{ padding: '12px' }}>
                    <button className="btn btn-secondary" onClick={() => handleEdit(flight)} style={{ marginRight: '8px', padding: '6px 12px' }}>
                      Düzenle
                    </button>
                    <button className="btn btn-danger" onClick={() => handleDelete(flight.id)} style={{ padding: '6px 12px' }}>
                      Sil
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <div className="card" style={{ marginTop: '24px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h3>Tüm Rezervasyonlar ({bookings.length})</h3>
          {bookings.length > 0 && (
            <button 
              className="btn btn-danger" 
              onClick={handleDeleteAllBookings}
              style={{ fontSize: '14px', padding: '8px 16px' }}
            >
              🗑️ Tüm Rezervasyonları Sil
            </button>
          )}
        </div>
        <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ background: '#f8f9fa' }}>
                <th style={{ padding: '12px', textAlign: 'left' }}>Rezervasyon No</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>Kullanıcı</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>Uçuş</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>Yolcu</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>Fiyat</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>Durum</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>Ödeme</th>
                <th style={{ padding: '12px', textAlign: 'left' }}>İşlem</th>
              </tr>
            </thead>
            <tbody>
              {bookings.map(booking => (
                <tr key={booking.id} style={{ borderBottom: '1px solid #e0e0e0' }}>
                  <td style={{ padding: '12px' }}>{booking.bookingReference}</td>
                  <td style={{ padding: '12px' }}>{booking.user?.email || 'Misafir'}</td>
                  <td style={{ padding: '12px' }}>{booking.flight?.flightNumber}</td>
                  <td style={{ padding: '12px' }}>{booking.passengerCount}</td>
                  <td style={{ padding: '12px' }}>₺{booking.totalPrice}</td>
                  <td style={{ padding: '12px' }}>
                    <span className={`booking-status status-${booking.status.toLowerCase()}`}>
                      {getStatusText(booking.status, booking.isPaid)}
                    </span>
                  </td>
                  <td style={{ padding: '12px' }}>{booking.isPaid ? '✅' : '❌'}</td>
                  <td style={{ padding: '12px' }}>
                    {booking.status === 'Confirmed' && (
                      <button 
                        className="btn btn-danger" 
                        onClick={() => handleCancelBooking(booking.id)}
                        style={{ padding: '6px 12px' }}
                      >
                        İptal Et
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

export default AdminDashboard
