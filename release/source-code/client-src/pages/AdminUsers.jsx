import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import axios from 'axios'

function AdminUsers() {
  const [users, setUsers] = useState([])
  const [stats, setStats] = useState({})
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [showEditModal, setShowEditModal] = useState(false)
  const [showUserDetail, setShowUserDetail] = useState(false)
  const [selectedUser, setSelectedUser] = useState(null)
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    phoneNumber: '',
    identityNumber: '',
    dateOfBirth: '',
    gender: 'Erkek',
    isAdmin: false
  })

  useEffect(() => {
    fetchUsers()
    fetchStats()
  }, [])

  // TC Kimlik numarası geçerlilik kontrolü
  const validateTCKimlik = (tcKimlik) => {
    if (!tcKimlik || tcKimlik.length !== 11) return false
    
    // İlk hane 0 olamaz
    if (tcKimlik[0] === '0') return false
    
    // Tüm haneler aynı olamaz
    if (tcKimlik.split('').every(digit => digit === tcKimlik[0])) return false
    
    // TC Kimlik algoritması
    const digits = tcKimlik.split('').map(Number)
    
    // İlk 10 hanenin toplamı
    const sum1to10 = digits.slice(0, 10).reduce((sum, digit) => sum + digit, 0)
    
    // 11. hane kontrolü
    if (sum1to10 % 10 !== digits[10]) return false
    
    // Tek ve çift haneler toplamı kontrolü
    const oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8]
    const evenSum = digits[1] + digits[3] + digits[5] + digits[7]
    
    // 10. hane kontrolü
    if (((oddSum * 7) - evenSum) % 10 !== digits[9]) return false
    
    return true
  }

  const fetchUsers = async () => {
    try {
      setLoading(true)
      setError('')
      const response = await axios.get('/api/users')
      setUsers(response.data)
    } catch (err) {
      console.error('Kullanıcılar yüklenirken hata:', err)
      if (err.response?.status === 401) {
        setError('Oturumunuzun süresi doldu. Lütfen tekrar giriş yapın.')
      } else {
        setError('Kullanıcılar yüklenirken hata oluştu: ' + (err.response?.data?.message || err.message))
      }
    } finally {
      setLoading(false)
    }
  }

  const fetchStats = async () => {
    try {
      const response = await axios.get('/api/users/stats')
      setStats(response.data)
    } catch (err) {
      console.error('İstatistikler yüklenirken hata:', err)
    }
  }

  const fetchUserDetail = async (userId) => {
    try {
      const response = await axios.get(`/api/users/${userId}`)
      setSelectedUser(response.data)
      setShowUserDetail(true)
    } catch (err) {
      console.error('User detail error:', err)
      if (err.response?.status === 401) {
        alert('⚠️ Oturumunuzun süresi doldu. Lütfen tekrar giriş yapın.')
        return
      }
      alert('Kullanıcı detayları yüklenirken hata oluştu: ' + (err.response?.data?.message || err.message))
    }
  }

  const handleCreateUser = async (e) => {
    e.preventDefault()
    try {
      await axios.post('/api/users', formData)
      alert('✅ Kullanıcı başarıyla oluşturuldu')
      setShowCreateModal(false)
      resetForm()
      fetchUsers()
      fetchStats()
    } catch (err) {
      console.error('Create user error:', err)
      alert('❌ ' + (err.response?.data?.message || 'Kullanıcı oluşturulurken hata oluştu'))
    }
  }

  const handleUpdateUser = async (e) => {
    e.preventDefault()
    try {
      await axios.put(`/api/users/${selectedUser.id}`, formData)
      alert('✅ Kullanıcı başarıyla güncellendi')
      setShowEditModal(false)
      resetForm()
      fetchUsers()
    } catch (err) {
      console.error('Update user error:', err)
      alert('❌ ' + (err.response?.data?.message || 'Kullanıcı güncellenirken hata oluştu'))
    }
  }

  const handleToggleStatus = async (userId) => {
    if (!confirm('Kullanıcının durumunu değiştirmek istediğinizden emin misiniz?')) return

    try {
      console.log('Sending PATCH request to:', `/api/users/${userId}/toggle-status`)
      const response = await axios.patch(`/api/users/${userId}/toggle-status`, {}, {
        headers: {
          'Content-Type': 'application/json'
        }
      })
      console.log('PATCH response:', response.data)
      alert('✅ ' + response.data.message + (response.data.tempPassword ? `\n🔑 Geçici şifre: ${response.data.tempPassword}` : ''))
      fetchUsers()
      fetchStats()
    } catch (err) {
      console.error('Toggle status error:', err)
      console.error('Error details:', {
        status: err.response?.status,
        statusText: err.response?.statusText,
        data: err.response?.data,
        message: err.message
      })
      
      if (err.response?.status === 401) {
        alert('⚠️ Oturumunuzun süresi doldu. Lütfen tekrar giriş yapın.')
        return
      }
      
      const errorMessage = err.response?.data?.message || err.message || 'İşlem sırasında hata oluştu'
      alert('❌ ' + errorMessage)
    }
  }

  const handleDeleteUser = async (userId, bookingCount = 0) => {
    const message = bookingCount > 0 
      ? `Bu kullanıcıyı ve ${bookingCount} rezervasyonunu silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.`
      : 'Bu kullanıcıyı silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.'
    
    if (!confirm(message)) return

    try {
      await axios.delete(`/api/users/${userId}`)
      alert('✅ Kullanıcı başarıyla silindi')
      fetchUsers()
      fetchStats()
    } catch (err) {
      console.error('Delete user error:', err)
      const errorMessage = err.response?.data?.message || 'Kullanıcı silinirken hata oluştu'
      alert('❌ ' + errorMessage)
    }
  }

  const openEditModal = (user) => {
    setSelectedUser(user)
    setFormData({
      firstName: user.firstName || '',
      lastName: user.lastName || '',
      email: user.email,
      password: '',
      phoneNumber: user.phoneNumber || '',
      identityNumber: user.identityNumber || '',
      dateOfBirth: user.dateOfBirth ? user.dateOfBirth.split('T')[0] : '',
      gender: user.gender || 'Erkek',
      isAdmin: user.isAdmin
    })
    setShowEditModal(true)
  }

  const resetForm = () => {
    setFormData({
      firstName: '',
      lastName: '',
      email: '',
      password: '',
      phoneNumber: '',
      identityNumber: '',
      dateOfBirth: '',
      gender: 'Erkek',
      isAdmin: false
    })
    setSelectedUser(null)
  }

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target
    let formattedValue = value

    // Telefon numarası formatlaması
    if (name === 'phoneNumber') {
      // Sadece rakamları al
      let numbers = value.replace(/\D/g, '')
      
      // Eğer 0 ile başlamıyorsa 0 ekle
      if (numbers.length > 0 && !numbers.startsWith('0')) {
        numbers = '0' + numbers
      }
      
      // Maksimum 11 hane
      if (numbers.length > 11) {
        numbers = numbers.slice(0, 11)
      }
      
      formattedValue = numbers
    }

    // TC Kimlik numarası formatlaması
    if (name === 'identityNumber') {
      // Sadece rakamları al
      let numbers = value.replace(/\D/g, '')
      
      // Maksimum 11 hane
      if (numbers.length > 11) {
        numbers = numbers.slice(0, 11)
      }
      
      formattedValue = numbers
    }

    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : formattedValue
    }))
  }

  if (loading) return <div className="loading">Yükleniyor...</div>

  return (
    <div className="container">
      <div style={{ marginBottom: '32px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '16px', marginBottom: '16px' }}>
          <Link 
            to="/admin" 
            style={{ 
              padding: '8px 16px', 
              background: '#667eea', 
              color: 'white', 
              textDecoration: 'none', 
              borderRadius: '8px',
              fontSize: '14px'
            }}
          >
            ← Admin Panel
          </Link>
          <h1 style={{ fontSize: '32px', fontWeight: '800', margin: 0, color: '#37474f' }}>
            👥 Kullanıcı Yönetimi
          </h1>
        </div>
        
        {/* İstatistikler */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '16px', marginBottom: '32px' }}>
          <div style={{ padding: '20px', background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', color: 'white', borderRadius: '16px', textAlign: 'center' }}>
            <div style={{ fontSize: '32px', fontWeight: 'bold' }}>{stats.totalUsers || 0}</div>
            <div style={{ fontSize: '14px', opacity: 0.9 }}>Toplam Kullanıcı</div>
          </div>
          <div style={{ padding: '20px', background: 'linear-gradient(135deg, #4caf50 0%, #388e3c 100%)', color: 'white', borderRadius: '16px', textAlign: 'center' }}>
            <div style={{ fontSize: '32px', fontWeight: 'bold' }}>{stats.activeUsers || 0}</div>
            <div style={{ fontSize: '14px', opacity: 0.9 }}>Aktif Kullanıcı</div>
          </div>
          <div style={{ padding: '20px', background: 'linear-gradient(135deg, #ff9800 0%, #f57c00 100%)', color: 'white', borderRadius: '16px', textAlign: 'center' }}>
            <div style={{ fontSize: '32px', fontWeight: 'bold' }}>{stats.passiveUsers || 0}</div>
            <div style={{ fontSize: '14px', opacity: 0.9 }}>Pasif Kullanıcı</div>
          </div>
          <div style={{ padding: '20px', background: 'linear-gradient(135deg, #9c27b0 0%, #7b1fa2 100%)', color: 'white', borderRadius: '16px', textAlign: 'center' }}>
            <div style={{ fontSize: '32px', fontWeight: 'bold' }}>{stats.guestUsers || 0}</div>
            <div style={{ fontSize: '14px', opacity: 0.9 }}>Misafir Kullanıcı</div>
          </div>
          <div style={{ padding: '20px', background: 'linear-gradient(135deg, #f44336 0%, #d32f2f 100%)', color: 'white', borderRadius: '16px', textAlign: 'center' }}>
            <div style={{ fontSize: '32px', fontWeight: 'bold' }}>{stats.adminUsers || 0}</div>
            <div style={{ fontSize: '14px', opacity: 0.9 }}>Admin Kullanıcı</div>
          </div>
          <div style={{ padding: '20px', background: 'linear-gradient(135deg, #00bcd4 0%, #00acc1 100%)', color: 'white', borderRadius: '16px', textAlign: 'center' }}>
            <div style={{ fontSize: '32px', fontWeight: 'bold' }}>{stats.newUsersThisMonth || 0}</div>
            <div style={{ fontSize: '14px', opacity: 0.9 }}>Bu Ay Yeni</div>
          </div>
        </div>

        <button 
          className="btn btn-primary" 
          onClick={() => setShowCreateModal(true)}
          style={{ marginBottom: '24px' }}
        >
          ➕ Yeni Kullanıcı Ekle
        </button>
      </div>

      {error && <div className="error">{error}</div>}

      {/* Kullanıcı Listesi */}
      <div className="card">
        <h2 style={{ marginBottom: '24px' }}>Kullanıcı Listesi</h2>
        <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ background: '#f8f9ff', borderBottom: '2px solid #e0e0e0' }}>
                <th style={{ padding: '12px', textAlign: 'left', fontWeight: 'bold' }}>Ad Soyad</th>
                <th style={{ padding: '12px', textAlign: 'left', fontWeight: 'bold' }}>E-posta</th>
                <th style={{ padding: '12px', textAlign: 'left', fontWeight: 'bold' }}>Telefon</th>
                <th style={{ padding: '12px', textAlign: 'center', fontWeight: 'bold' }}>Tip</th>
                <th style={{ padding: '12px', textAlign: 'center', fontWeight: 'bold' }}>Durum</th>
                <th style={{ padding: '12px', textAlign: 'center', fontWeight: 'bold' }}>Rezervasyon</th>
                <th style={{ padding: '12px', textAlign: 'center', fontWeight: 'bold' }}>Kayıt Tarihi</th>
                <th style={{ padding: '12px', textAlign: 'center', fontWeight: 'bold' }}>İşlemler</th>
              </tr>
            </thead>
            <tbody>
              {users.map(user => (
                <tr key={user.id} style={{ borderBottom: '1px solid #e0e0e0' }}>
                  <td style={{ padding: '12px' }}>
                    <div style={{ fontWeight: 'bold' }}>
                      {user.firstName && user.lastName ? `${user.firstName} ${user.lastName}` : 'Belirtilmemiş'}
                    </div>
                    {user.identityNumber && (
                      <div style={{ fontSize: '12px', color: '#666' }}>TC: {user.identityNumber}</div>
                    )}
                  </td>
                  <td style={{ padding: '12px' }}>{user.email}</td>
                  <td style={{ padding: '12px' }}>{user.phoneNumber || '-'}</td>
                  <td style={{ padding: '12px', textAlign: 'center' }}>
                    <span style={{
                      padding: '4px 8px',
                      borderRadius: '12px',
                      fontSize: '12px',
                      fontWeight: 'bold',
                      background: user.isAdmin ? '#f44336' : user.isGuest ? '#9c27b0' : '#2196f3',
                      color: 'white'
                    }}>
                      {user.isAdmin ? '👑 Admin' : user.isGuest ? '👤 Misafir' : '👨‍💼 Üye'}
                    </span>
                  </td>
                  <td style={{ padding: '12px', textAlign: 'center' }}>
                    <span style={{
                      padding: '4px 8px',
                      borderRadius: '12px',
                      fontSize: '12px',
                      fontWeight: 'bold',
                      background: user.isActive ? '#4caf50' : '#ff9800',
                      color: 'white'
                    }}>
                      {user.isActive ? '✅ Aktif' : '⏸️ Pasif'}
                    </span>
                  </td>
                  <td style={{ padding: '12px', textAlign: 'center' }}>
                    <span style={{
                      padding: '4px 8px',
                      borderRadius: '12px',
                      fontSize: '12px',
                      fontWeight: 'bold',
                      background: user.bookingCount > 0 ? '#ff9800' : '#4caf50',
                      color: 'white'
                    }}>
                      {user.bookingCount} Rezervasyon
                    </span>
                    {user.bookingCount > 0 && (
                      <div style={{ fontSize: '10px', color: '#ff9800', marginTop: '2px' }}>
                        ⚠️ Rezervasyonlarla birlikte silinir
                      </div>
                    )}
                  </td>
                  <td style={{ padding: '12px', textAlign: 'center', fontSize: '12px', color: '#666' }}>
                    {new Date(user.createdAt).toLocaleDateString('tr-TR')}
                  </td>
                  <td style={{ padding: '12px', textAlign: 'center' }}>
                    <div style={{ display: 'flex', gap: '8px', justifyContent: 'center', flexWrap: 'wrap' }}>
                      <button
                        className="btn btn-sm"
                        onClick={() => fetchUserDetail(user.id)}
                        style={{ background: '#2196f3', color: 'white', fontSize: '12px', padding: '4px 8px' }}
                      >
                        👁️ Detay
                      </button>
                      <button
                        className="btn btn-sm"
                        onClick={() => openEditModal(user)}
                        style={{ background: '#ff9800', color: 'white', fontSize: '12px', padding: '4px 8px' }}
                      >
                        ✏️ Düzenle
                      </button>
                      {!user.isGuest && (
                        <button
                          className="btn btn-sm"
                          onClick={() => handleToggleStatus(user.id)}
                          style={{ 
                            background: user.isActive ? '#f44336' : '#4caf50', 
                            color: 'white', 
                            fontSize: '12px', 
                            padding: '4px 8px' 
                          }}
                        >
                          {user.isActive ? '⏸️ Pasifleştir' : '▶️ Aktifleştir'}
                        </button>
                      )}
                      <button
                        className="btn btn-sm"
                        onClick={() => handleDeleteUser(user.id, user.bookingCount)}
                        style={{ 
                          background: '#f44336', 
                          color: 'white', 
                          fontSize: '12px', 
                          padding: '4px 8px'
                        }}
                        title={user.bookingCount > 0 ? 'Kullanıcıyı ve rezervasyonlarını sil' : 'Kullanıcıyı sil'}
                      >
                        🗑️ Sil
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Kullanıcı Oluşturma Modal */}
      {showCreateModal && (
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
            maxWidth: '600px',
            width: '100%',
            maxHeight: '90vh',
            overflow: 'auto'
          }}>
            <h2 style={{ marginBottom: '24px', textAlign: 'center' }}>➕ Yeni Kullanıcı Ekle</h2>
            <form onSubmit={handleCreateUser}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                <div className="form-group">
                  <label>Ad</label>
                  <input
                    type="text"
                    name="firstName"
                    value={formData.firstName}
                    onChange={handleInputChange}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Soyad</label>
                  <input
                    type="text"
                    name="lastName"
                    value={formData.lastName}
                    onChange={handleInputChange}
                    required
                  />
                </div>
              </div>
              
              <div className="form-group">
                <label>E-posta</label>
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleInputChange}
                  required
                />
              </div>
              
              <div className="form-group">
                <label>Şifre</label>
                <input
                  type="password"
                  name="password"
                  value={formData.password}
                  onChange={handleInputChange}
                  placeholder="Boş bırakılırsa şifresiz oluşturulur"
                />
              </div>
              
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                <div className="form-group">
                  <label>Telefon</label>
                  <input
                    type="tel"
                    name="phoneNumber"
                    value={formData.phoneNumber}
                    onChange={handleInputChange}
                    placeholder="05xxxxxxxxx"
                    maxLength="11"
                    pattern="0[0-9]{10}"
                  />
                  <small style={{ color: '#666', fontSize: '12px' }}>
                    0 ile başlayan 11 haneli telefon numarası
                  </small>
                </div>
                <div className="form-group">
                  <label>TC Kimlik No</label>
                  <input
                    type="text"
                    name="identityNumber"
                    value={formData.identityNumber}
                    onChange={handleInputChange}
                    maxLength="11"
                  />
                </div>
              </div>
              
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                <div className="form-group">
                  <label>Doğum Tarihi</label>
                  <input
                    type="date"
                    name="dateOfBirth"
                    value={formData.dateOfBirth}
                    onChange={handleInputChange}
                  />
                </div>
                <div className="form-group">
                  <label>Cinsiyet</label>
                  <select
                    name="gender"
                    value={formData.gender}
                    onChange={handleInputChange}
                  >
                    <option value="Erkek">Erkek</option>
                    <option value="Kadın">Kadın</option>
                  </select>
                </div>
              </div>
              
              <div style={{ display: 'flex', gap: '12px', marginTop: '24px' }}>
                <button type="submit" className="btn btn-primary" style={{ flex: 1 }}>
                  ✅ Kullanıcı Oluştur
                </button>
                <button 
                  type="button" 
                  className="btn btn-secondary"
                  onClick={() => { setShowCreateModal(false); resetForm(); }}
                >
                  İptal
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Kullanıcı Düzenleme Modal */}
      {showEditModal && (
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
            borderRadius: '16px',
            padding: '24px',
            maxWidth: '500px',
            width: '100%',
            maxHeight: '85vh',
            overflow: 'auto'
          }}>
            <h2 style={{ marginBottom: '20px', textAlign: 'center', fontSize: '20px' }}>✏️ Kullanıcı Düzenle</h2>
            <form onSubmit={handleUpdateUser}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                <div className="form-group">
                  <label>Ad</label>
                  <input
                    type="text"
                    name="firstName"
                    value={formData.firstName}
                    onChange={handleInputChange}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Soyad</label>
                  <input
                    type="text"
                    name="lastName"
                    value={formData.lastName}
                    onChange={handleInputChange}
                    required
                  />
                </div>
              </div>
              
              <div className="form-group">
                <label>E-posta</label>
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleInputChange}
                  required
                />
              </div>
              
              <div className="form-group">
                <label>Yeni Şifre</label>
                <input
                  type="password"
                  name="password"
                  value={formData.password}
                  onChange={handleInputChange}
                  placeholder="Değiştirmek istemiyorsanız boş bırakın"
                />
              </div>
              
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                <div className="form-group">
                  <label>Telefon</label>
                  <input
                    type="tel"
                    name="phoneNumber"
                    value={formData.phoneNumber}
                    onChange={handleInputChange}
                    placeholder="05xxxxxxxxx"
                    maxLength="11"
                    pattern="0[0-9]{10}"
                  />
                  <small style={{ color: '#666', fontSize: '12px' }}>
                    0 ile başlayan 11 haneli telefon numarası
                  </small>
                </div>
                <div className="form-group">
                  <label>TC Kimlik No</label>
                  <input
                    type="text"
                    name="identityNumber"
                    value={formData.identityNumber}
                    onChange={handleInputChange}
                    maxLength="11"
                  />
                </div>
              </div>
              
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                <div className="form-group">
                  <label>Doğum Tarihi</label>
                  <input
                    type="date"
                    name="dateOfBirth"
                    value={formData.dateOfBirth}
                    onChange={handleInputChange}
                  />
                </div>
                <div className="form-group">
                  <label>Cinsiyet</label>
                  <select
                    name="gender"
                    value={formData.gender}
                    onChange={handleInputChange}
                  >
                    <option value="Erkek">Erkek</option>
                    <option value="Kadın">Kadın</option>
                  </select>
                </div>
              </div>
              
              <div style={{ display: 'flex', gap: '12px', marginTop: '24px' }}>
                <button type="submit" className="btn btn-primary" style={{ flex: 1 }}>
                  ✅ Güncelle
                </button>
                <button 
                  type="button" 
                  className="btn btn-secondary"
                  onClick={() => { setShowEditModal(false); resetForm(); }}
                >
                  İptal
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Kullanıcı Detay Modal */}
      {showUserDetail && selectedUser && (
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
            maxWidth: '800px',
            width: '100%',
            maxHeight: '90vh',
            overflow: 'auto'
          }}>
            <h2 style={{ marginBottom: '24px', textAlign: 'center' }}>👤 Kullanıcı Detayları</h2>
            
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '16px', marginBottom: '24px' }}>
              <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
                <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Ad Soyad</div>
                <div style={{ fontSize: '16px', fontWeight: 'bold' }}>
                  {selectedUser.firstName && selectedUser.lastName ? `${selectedUser.firstName} ${selectedUser.lastName}` : 'Belirtilmemiş'}
                </div>
              </div>
              <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
                <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>E-posta</div>
                <div style={{ fontSize: '16px', fontWeight: 'bold' }}>{selectedUser.email}</div>
              </div>
              <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
                <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Telefon</div>
                <div style={{ fontSize: '16px', fontWeight: 'bold' }}>{selectedUser.phoneNumber || 'Belirtilmemiş'}</div>
              </div>
              <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
                <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>TC Kimlik</div>
                <div style={{ fontSize: '16px', fontWeight: 'bold' }}>{selectedUser.identityNumber || 'Belirtilmemiş'}</div>
              </div>
              <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
                <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Doğum Tarihi</div>
                <div style={{ fontSize: '16px', fontWeight: 'bold' }}>
                  {selectedUser.dateOfBirth ? new Date(selectedUser.dateOfBirth).toLocaleDateString('tr-TR') : 'Belirtilmemiş'}
                </div>
              </div>
              <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
                <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Cinsiyet</div>
                <div style={{ fontSize: '16px', fontWeight: 'bold' }}>{selectedUser.gender || 'Belirtilmemiş'}</div>
              </div>
              <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
                <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Kullanıcı Tipi</div>
                <div style={{ fontSize: '16px', fontWeight: 'bold' }}>
                  {selectedUser.isAdmin ? '👑 Admin' : selectedUser.isGuest ? '👤 Misafir' : '👨‍💼 Üye'}
                </div>
              </div>
              <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
                <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Durum</div>
                <div style={{ fontSize: '16px', fontWeight: 'bold', color: selectedUser.isActive ? '#4caf50' : '#ff9800' }}>
                  {selectedUser.isActive ? '✅ Aktif' : '⏸️ Pasif'}
                </div>
              </div>
              <div style={{ padding: '16px', background: '#f8f9ff', borderRadius: '12px' }}>
                <div style={{ fontSize: '12px', color: '#666', marginBottom: '4px' }}>Kayıt Tarihi</div>
                <div style={{ fontSize: '16px', fontWeight: 'bold' }}>
                  {new Date(selectedUser.createdAt).toLocaleDateString('tr-TR')}
                </div>
              </div>
            </div>

            {/* Rezervasyonlar */}
            {selectedUser.bookings && selectedUser.bookings.length > 0 && (
              <div style={{ marginBottom: '24px' }}>
                <h3 style={{ marginBottom: '16px' }}>✈️ Rezervasyonlar ({selectedUser.bookings.length})</h3>
                <div style={{ display: 'grid', gap: '12px' }}>
                  {selectedUser.bookings.map((booking, index) => (
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
                        <div style={{ fontSize: '16px', fontWeight: 'bold' }}>
                          PNR: {booking.bookingReference}
                        </div>
                        <div style={{ fontSize: '14px', color: '#666' }}>
                          {booking.flightNumber} • {booking.route}
                        </div>
                        <div style={{ fontSize: '12px', color: '#888' }}>
                          {new Date(booking.bookingDate).toLocaleDateString('tr-TR')}
                        </div>
                      </div>
                      <div style={{ textAlign: 'right' }}>
                        <div style={{ fontSize: '18px', fontWeight: 'bold', color: '#667eea' }}>
                          ₺{booking.totalPrice}
                        </div>
                        <div style={{
                          fontSize: '12px',
                          fontWeight: 'bold',
                          color: booking.isPaid ? '#4caf50' : booking.status === 'Cancelled' ? '#f44336' : '#ff9800'
                        }}>
                          {booking.isPaid ? '✅ Ödendi' : booking.status === 'Cancelled' ? '❌ İptal' : '⏳ Bekliyor'}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}
            
            <div style={{ textAlign: 'center' }}>
              <button 
                className="btn btn-secondary"
                onClick={() => setShowUserDetail(false)}
              >
                Kapat
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default AdminUsers