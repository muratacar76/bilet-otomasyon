import { useState, useEffect } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import axios from 'axios'

function Register({ setUser }) {
  const location = useLocation()
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: location.state?.email || '',
    password: '',
    confirmPassword: '',
    phoneNumber: ''
  })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [showWelcome, setShowWelcome] = useState(!!location.state?.email)
  const navigate = useNavigate()

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value })
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    // Şifre kontrolü
    if (formData.password !== formData.confirmPassword) {
      setError('Şifreler eşleşmiyor')
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
            />
          </div>
          <div className="form-group">
            <label>Soyad</label>
            <input
              type="text"
              name="lastName"
              value={formData.lastName}
              onChange={handleChange}
              required
            />
          </div>
          <div className="form-group">
            <label>E-posta</label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
              readOnly={!!location.state?.email}
              style={location.state?.email ? { background: '#f0f0f0', cursor: 'not-allowed' } : {}}
            />
            {location.state?.email && (
              <small style={{ color: '#666', fontSize: '12px' }}>
                ✅ Rezervasyon yaptığınız e-posta adresi
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
            />
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
            />
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
          <button type="submit" className="btn btn-primary" disabled={loading} style={{ width: '100%' }}>
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
