import { useState, useEffect } from 'react'
import { useNavigate, useLocation, useSearchParams } from 'react-router-dom'
import axios from 'axios'

function Register({ setUser }) {
  const location = useLocation()
  const [searchParams] = useSearchParams()
  const [formData, setFormData] = useState({
    firstName: location.state?.firstName || '',
    lastName: location.state?.lastName || '',
    email: location.state?.email || searchParams.get('email') || '',
    password: '',
    confirmPassword: '',
    phoneNumber: location.state?.phoneNumber || '',
    identityNumber: location.state?.identityNumber || '',
    dateOfBirth: location.state?.dateOfBirth || '',
    gender: location.state?.gender || 'Erkek'
  })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [showWelcome] = useState(!!(location.state?.email || location.state?.firstName || location.state?.lastName || location.state?.phoneNumber || location.state?.identityNumber || location.state?.dateOfBirth || location.state?.gender || searchParams.get('email')))
  const navigate = useNavigate()

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value })
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    // Şifre boş kontrolü
    if (!formData.password || formData.password.length < 4) {
      setError('Şifre en az 4 karakter olmalıdır')
      setLoading(false)
      return
    }

    // Şifre eşleşme kontrolü
    if (formData.password !== formData.confirmPassword) {
      setError('Şifreler eşleşmiyor')
      setLoading(false)
      return
    }

    // Şifre tekrarı boş kontrolü
    if (!formData.confirmPassword) {
      setError('Şifre tekrarını giriniz')
      setLoading(false)
      return
    }

    // Zorunlu alanların dolu olup olmadığını kontrol et
    if (!formData.firstName || !formData.lastName || !formData.email) {
      setError('Ad, soyad ve e-posta alanları zorunludur')
      setLoading(false)
      return
    }

    try {
      // confirmPassword'u API'ye göndermiyoruz
      const { confirmPassword, ...dataToSend } = formData
      const response = await axios.post('/api/auth/register', dataToSend)
      localStorage.setItem('token', response.data.token)
      localStorage.setItem('user', JSON.stringify(response.data.user))
      setUser(response.data.user)
      navigate('/flights')
    } catch (err) {
      console.error('Kayıt hatası:', err)
      setError(err.response?.data?.message || err.message || 'Kayıt başarısız')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="container">
      <div className="card" style={{ maxWidth: '500px', margin: '40px auto' }}>
        {showWelcome && (
          <div style={{
            background: 'linear-gradient(135deg, #4caf50 0%, #388e3c 100%)',
            color: 'white',
            padding: '24px',
            borderRadius: '16px',
            marginBottom: '32px',
            textAlign: 'center',
            animation: 'slideInLeft 0.5s ease-out'
          }}>
            <h3 style={{ fontSize: '26px', marginBottom: '16px', fontWeight: '700' }}>🎉 Hoş Geldiniz!</h3>
            <p style={{ fontSize: '16px', lineHeight: '1.6' }}>Rezervasyonunuz tamamlandı. Şimdi üyeliğinizi tamamlayın ve avantajlardan yararlanın!</p>
          </div>
        )}
        <h2 style={{ marginBottom: '32px', textAlign: 'center', fontSize: '28px', fontWeight: '700', color: '#37474f' }}>
          BULUTBİLET<span style={{ color: '#00e5ff' }}>.COM</span> - {showWelcome ? '✨ Üyeliğinizi Tamamlayın' : 'Kayıt Ol'}
        </h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Ad</label>
            <input
              type="text"
              name="firstName"
              value={formData.firstName}
              onChange={handleChange}
              required
              readOnly={!!location.state?.firstName}
              style={location.state?.firstName ? { background: '#f0f0f0', cursor: 'not-allowed' } : {}}
            />
            {location.state?.firstName && (
              <small style={{ color: '#666', fontSize: '12px' }}>
                ✅ Rezervasyon bilgilerinizden alındı
              </small>
            )}
          </div>
          <div className="form-group">
            <label>Soyad</label>
            <input
              type="text"
              name="lastName"
              value={formData.lastName}
              onChange={handleChange}
              required
              readOnly={!!location.state?.lastName}
              style={location.state?.lastName ? { background: '#f0f0f0', cursor: 'not-allowed' } : {}}
            />
            {location.state?.lastName && (
              <small style={{ color: '#666', fontSize: '12px' }}>
                ✅ Rezervasyon bilgilerinizden alındı
              </small>
            )}
          </div>
          <div className="form-group">
            <label>TC Kimlik Numarası</label>
            <input
              type="text"
              name="identityNumber"
              value={formData.identityNumber}
              onChange={handleChange}
              maxLength="11"
              placeholder="TC Kimlik No (11 hane)"
              readOnly={!!location.state?.identityNumber}
              style={location.state?.identityNumber ? { background: '#f0f0f0', cursor: 'not-allowed' } : {}}
            />
            {location.state?.identityNumber && (
              <small style={{ color: '#666', fontSize: '12px' }}>
                ✅ Rezervasyon bilgilerinizden alındı
              </small>
            )}
          </div>
          <div className="form-group">
            <label>Telefon</label>
            <input
              type="tel"
              name="phoneNumber"
              value={formData.phoneNumber}
              onChange={handleChange}
              readOnly={!!location.state?.phoneNumber}
              style={location.state?.phoneNumber ? { background: '#f0f0f0', cursor: 'not-allowed' } : {}}
            />
            {location.state?.phoneNumber && (
              <small style={{ color: '#666', fontSize: '12px' }}>
                ✅ Rezervasyon bilgilerinizden alındı
              </small>
            )}
          </div>
          <div className="form-group">
            <label>Doğum Tarihi</label>
            <input
              type="date"
              name="dateOfBirth"
              value={formData.dateOfBirth}
              onChange={handleChange}
              max={new Date().toISOString().split('T')[0]}
              readOnly={!!location.state?.dateOfBirth}
              style={location.state?.dateOfBirth ? { background: '#f0f0f0', cursor: 'not-allowed' } : {}}
            />
            {location.state?.dateOfBirth && (
              <small style={{ color: '#666', fontSize: '12px' }}>
                ✅ Rezervasyon bilgilerinizden alındı
              </small>
            )}
          </div>
          <div className="form-group">
            <label>Cinsiyet</label>
            <select
              name="gender"
              value={formData.gender}
              onChange={handleChange}
              disabled={!!location.state?.gender}
              style={location.state?.gender ? { background: '#f0f0f0', cursor: 'not-allowed' } : {}}
            >
              <option value="Erkek">Erkek</option>
              <option value="Kadın">Kadın</option>
            </select>
            {location.state?.gender && (
              <small style={{ color: '#666', fontSize: '12px' }}>
                ✅ Rezervasyon bilgilerinizden alındı
              </small>
            )}
          </div>
          <div className="form-group">
            <label>E-posta</label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
              readOnly={!!(location.state?.email || searchParams.get('email'))}
              style={(location.state?.email || searchParams.get('email')) ? { background: '#f0f0f0', cursor: 'not-allowed' } : {}}
            />
            {(location.state?.email || searchParams.get('email')) && (
              <small style={{ color: '#666', fontSize: '12px' }}>
                ✅ Rezervasyon yaptığınız e-posta adresi
              </small>
            )}
          </div>
          <div className="form-group">
            <label>Şifre</label>
            <input
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              required
              minLength="4"
              placeholder="En az 4 karakter"
              style={{
                borderColor: formData.password && formData.password.length < 4 ? '#dc3545' : 
                           formData.password && formData.password.length >= 4 ? '#28a745' : ''
              }}
            />
            {formData.password && formData.password.length < 4 && (
              <small style={{ color: '#dc3545', fontSize: '12px' }}>
                ❌ Şifre en az 4 karakter olmalıdır ({formData.password.length}/4)
              </small>
            )}
            {formData.password && formData.password.length >= 4 && (
              <small style={{ color: '#28a745', fontSize: '12px' }}>
                ✅ Şifre uygun
              </small>
            )}
          </div>
          <div className="form-group">
            <label>Şifre Tekrarı</label>
            <input
              type="password"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              required
              minLength="4"
              style={{
                borderColor: formData.confirmPassword && formData.password !== formData.confirmPassword ? '#dc3545' : ''
              }}
            />
            {formData.confirmPassword && formData.password !== formData.confirmPassword && (
              <small style={{ color: '#dc3545', fontSize: '12px' }}>
                ❌ Şifreler eşleşmiyor
              </small>
            )}
            {formData.confirmPassword && formData.password === formData.confirmPassword && (
              <small style={{ color: '#28a745', fontSize: '12px' }}>
                ✅ Şifreler eşleşiyor
              </small>
            )}
          </div>
          {error && <div className="error">{error}</div>}
          <button 
            type="submit" 
            className="btn btn-primary" 
            disabled={loading || !formData.password || !formData.confirmPassword || formData.password !== formData.confirmPassword || formData.password.length < 4} 
            style={{ 
              width: '100%',
              opacity: (loading || !formData.password || !formData.confirmPassword || formData.password !== formData.confirmPassword || formData.password.length < 4) ? 0.6 : 1
            }}
          >
            {loading ? 'Kayıt yapılıyor...' : 'Kayıt Ol'}
          </button>
        </form>
        <p style={{ textAlign: 'center', marginTop: '20px' }}>
          Zaten hesabınız var mı? <a href="/login">Giriş yapın</a>
        </p>
      </div>
    </div>
  )
}

export default Register
