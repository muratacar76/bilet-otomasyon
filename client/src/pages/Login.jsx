import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'

function Login({ setUser }) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      const response = await axios.post('/api/auth/login', { email, password })
      localStorage.setItem('token', response.data.token)
      localStorage.setItem('user', JSON.stringify(response.data.user))
      setUser(response.data.user)
      navigate('/flights')
    } catch (err) {
      setError(err.response?.data?.message || 'Giriş başarısız')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="container">
      <div className="card" style={{ maxWidth: '500px', margin: '40px auto' }}>
        <h2 style={{ marginBottom: '24px', textAlign: 'center' }}>Giriş Yap</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>E-posta</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              placeholder="ornek@email.com"
            />
          </div>
          <div className="form-group">
            <label>Şifre</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              placeholder="••••••••"
            />
          </div>
          {error && <div className="error">{error}</div>}
          <button type="submit" className="btn btn-primary" disabled={loading} style={{ width: '100%' }}>
            {loading ? 'Giriş yapılıyor...' : 'Giriş Yap'}
          </button>
        </form>
        <p style={{ textAlign: 'center', marginTop: '20px' }}>
          Hesabınız yok mu? <a href="/register">Kayıt olun</a>
        </p>
      </div>
    </div>
  )
}

export default Login
