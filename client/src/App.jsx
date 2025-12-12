import { BrowserRouter as Router, Routes, Route, Link, Navigate } from 'react-router-dom'
import { useState, useEffect } from 'react'
import Home from './pages/Home'
import Login from './pages/Login'
import Register from './pages/Register'
import Flights from './pages/Flights'
import Bookings from './pages/Bookings'
import AdminDashboard from './pages/AdminDashboard'
import GuestBooking from './pages/GuestBooking'
import Privacy from './pages/Privacy'
import Terms from './pages/Terms'
import Cookies from './pages/Cookies'
import KVKK from './pages/KVKK'
import Footer from './components/Footer'

function App() {
  const [user, setUser] = useState(null)

  useEffect(() => {
    const token = localStorage.getItem('token')
    const userData = localStorage.getItem('user')
    if (token && userData) {
      setUser(JSON.parse(userData))
    }
  }, [])

  const logout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    setUser(null)
  }

  return (
    <Router>
      <div className="navbar">
        <div className="navbar-content">
          <Link to="/" style={{ textDecoration: 'none' }}>
            <h1>
              BULUTBİLET<span style={{ color: '#00e5ff' }}>.COM</span>
            </h1>
          </Link>
          <div className="navbar-links">
            <Link to="/flights">Uçuşlar</Link>
            <Link to="/guest-booking">🎫 PNR Sorgula</Link>
            {user && <Link to="/bookings">Rezervasyonlarım</Link>}
            {user?.isAdmin && <Link to="/admin">Admin Panel</Link>}
            {!user ? (
              <>
                <Link to="/login">Giriş Yap</Link>
                <Link to="/register">Kayıt Ol</Link>
              </>
            ) : (
              <>
                <span>Hoş geldin, {user.firstName || user.email}</span>
                <button onClick={logout} className="btn btn-secondary">Çıkış</button>
              </>
            )}
          </div>
        </div>
      </div>

      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login setUser={setUser} />} />
        <Route path="/register" element={<Register setUser={setUser} />} />
        <Route path="/flights" element={<Flights user={user} />} />
        <Route path="/guest-booking" element={<GuestBooking user={user} />} />
        <Route path="/bookings" element={user ? <Bookings /> : <Navigate to="/login" />} />
        <Route path="/admin" element={user?.isAdmin ? <AdminDashboard /> : <Navigate to="/" />} />
        <Route path="/privacy" element={<Privacy />} />
        <Route path="/terms" element={<Terms />} />
        <Route path="/cookies" element={<Cookies />} />
        <Route path="/kvkk" element={<KVKK />} />
      </Routes>
      
      <Footer user={user} />
    </Router>
  )
}

export default App
