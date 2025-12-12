// Admin panel JavaScript functionality

// API Base URL
const API_BASE = '/api';

// Admin utilities
class AdminAPI {
    static async request(url, options = {}) {
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            credentials: 'same-origin' // Include cookies for authentication
        };

        const response = await fetch(API_BASE + url, {
            ...defaultOptions,
            ...options,
            headers: { ...defaultOptions.headers, ...options.headers }
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(error || 'İşlem başarısız');
        }

        const contentType = response.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            return response.json();
        }
        return response.text();
    }

    // Flight operations
    static async getFlights() {
        return this.request('/flights');
    }

    static async createFlight(flightData) {
        return this.request('/flights', {
            method: 'POST',
            body: JSON.stringify(flightData)
        });
    }

    static async updateFlight(id, flightData) {
        return this.request(`/flights/${id}`, {
            method: 'PUT',
            body: JSON.stringify(flightData)
        });
    }

    static async deleteFlight(id) {
        return this.request(`/flights/${id}`, {
            method: 'DELETE'
        });
    }

    // Booking operations
    static async getAllBookings() {
        return this.request('/bookings/all');
    }

    static async cancelBooking(id) {
        return this.request(`/bookings/${id}`, {
            method: 'DELETE'
        });
    }

    static async deleteAllBookings() {
        return this.request('/bookings/all', {
            method: 'DELETE'
        });
    }

    // User operations
    static async getUsers() {
        return this.request('/users');
    }

    static async createUser(userData) {
        return this.request('/users', {
            method: 'POST',
            body: JSON.stringify(userData)
        });
    }

    static async updateUser(id, userData) {
        return this.request(`/users/${id}`, {
            method: 'PUT',
            body: JSON.stringify(userData)
        });
    }

    static async toggleUserStatus(id) {
        return this.request(`/users/${id}/toggle-status`, {
            method: 'PATCH'
        });
    }

    static async deleteUser(id) {
        return this.request(`/users/${id}`, {
            method: 'DELETE'
        });
    }

    static async getUserStats() {
        return this.request('/users/stats');
    }
}

// Flight management utilities
class FlightManager {
    constructor() {
        this.flights = [];
        this.editingId = null;
    }

    async loadFlights() {
        try {
            this.flights = await AdminAPI.getFlights();
            this.renderFlights();
        } catch (error) {
            console.error('Uçuşlar yüklenemedi:', error);
            this.showAlert('Uçuşlar yüklenirken hata oluştu', 'error');
        }
    }

    renderFlights() {
        const tbody = document.getElementById('flights-tbody');
        if (!tbody) return;

        tbody.innerHTML = '';

        this.flights.forEach(flight => {
            const row = this.createFlightRow(flight);
            tbody.appendChild(row);
        });
    }

    createFlightRow(flight) {
        const row = document.createElement('tr');
        row.style.borderBottom = '1px solid #e0e0e0';
        row.innerHTML = `
            <td style="padding: 12px;">${flight.flightNumber}</td>
            <td style="padding: 12px;">${flight.airline}</td>
            <td style="padding: 12px;">${flight.departureCity} → ${flight.arrivalCity}</td>
            <td style="padding: 12px;">${new Date(flight.departureTime).toLocaleString('tr-TR')}</td>
            <td style="padding: 12px;">₺${flight.price}</td>
            <td style="padding: 12px;">${flight.availableSeats}/${flight.totalSeats}</td>
            <td style="padding: 12px;">
                <button class="btn btn-secondary" onclick="flightManager.editFlight(${flight.id})" style="margin-right: 8px; padding: 6px 12px;">
                    Düzenle
                </button>
                <button class="btn btn-danger" onclick="flightManager.deleteFlight(${flight.id})" style="padding: 6px 12px;">
                    Sil
                </button>
            </td>
        `;
        return row;
    }

    async createFlight(flightData) {
        try {
            await AdminAPI.createFlight(flightData);
            this.showAlert('Uçuş başarıyla eklendi', 'success');
            this.loadFlights();
        } catch (error) {
            this.showAlert(error.message, 'error');
        }
    }

    async updateFlight(id, flightData) {
        try {
            await AdminAPI.updateFlight(id, flightData);
            this.showAlert('Uçuş başarıyla güncellendi', 'success');
            this.loadFlights();
        } catch (error) {
            this.showAlert(error.message, 'error');
        }
    }

    async deleteFlight(id) {
        if (!confirm('⚠️ Uçuşu silmek istediğinizden emin misiniz?')) return;

        try {
            await AdminAPI.deleteFlight(id);
            this.showAlert('Uçuş başarıyla silindi', 'success');
            this.loadFlights();
        } catch (error) {
            this.showAlert(error.message, 'error');
        }
    }

    editFlight(id) {
        const flight = this.flights.find(f => f.id === id);
        if (!flight) return;

        this.editingId = id;
        this.populateFlightForm(flight);
        this.showFlightForm();
    }

    populateFlightForm(flight) {
        const form = document.getElementById('flight-form-element');
        if (!form) return;

        form.querySelector('#flightNumber').value = flight.flightNumber;
        form.querySelector('#airline').value = flight.airline;
        form.querySelector('#departureCity').value = flight.departureCity;
        form.querySelector('#arrivalCity').value = flight.arrivalCity;
        form.querySelector('#departureTime').value = flight.departureTime.slice(0, 16);
        form.querySelector('#arrivalTime').value = flight.arrivalTime.slice(0, 16);
        form.querySelector('#price').value = flight.price;
        form.querySelector('#totalSeats').value = flight.totalSeats;

        document.getElementById('flight-form-title').textContent = 'Uçuş Düzenle';
        document.getElementById('flight-submit-btn').textContent = 'Güncelle';
    }

    showFlightForm() {
        const form = document.getElementById('flight-form');
        const toggleText = document.getElementById('flight-form-toggle-text');
        
        if (form && toggleText) {
            form.style.display = 'block';
            toggleText.textContent = 'İptal';
        }
    }

    hideFlightForm() {
        const form = document.getElementById('flight-form');
        const toggleText = document.getElementById('flight-form-toggle-text');
        
        if (form && toggleText) {
            form.style.display = 'none';
            toggleText.textContent = 'Yeni Uçuş Ekle';
            this.resetFlightForm();
        }
    }

    resetFlightForm() {
        const form = document.getElementById('flight-form-element');
        if (form) {
            form.reset();
            document.getElementById('flight-form-title').textContent = 'Yeni Uçuş';
            document.getElementById('flight-submit-btn').textContent = 'Ekle';
            this.editingId = null;
        }
    }

    showAlert(message, type = 'info') {
        alert((type === 'success' ? '✅ ' : type === 'error' ? '❌ ' : 'ℹ️ ') + message);
    }
}

// Booking management utilities
class BookingManager {
    constructor() {
        this.bookings = [];
    }

    async loadBookings() {
        try {
            this.bookings = await AdminAPI.getAllBookings();
            this.renderBookings();
        } catch (error) {
            console.error('Rezervasyonlar yüklenemedi:', error);
            this.showAlert('Rezervasyonlar yüklenirken hata oluştu', 'error');
        }
    }

    renderBookings() {
        const tbody = document.getElementById('bookings-tbody');
        const countSpan = document.getElementById('bookings-count');
        
        if (!tbody) return;

        tbody.innerHTML = '';
        if (countSpan) countSpan.textContent = this.bookings.length;

        this.bookings.forEach(booking => {
            const row = this.createBookingRow(booking);
            tbody.appendChild(row);
        });
    }

    createBookingRow(booking) {
        const statusText = this.getStatusText(booking.status, booking.isPaid);
        const row = document.createElement('tr');
        row.style.borderBottom = '1px solid #e0e0e0';
        row.innerHTML = `
            <td style="padding: 12px;">${booking.bookingReference}</td>
            <td style="padding: 12px;">${booking.user?.email || 'Misafir'}</td>
            <td style="padding: 12px;">${booking.flight?.flightNumber}</td>
            <td style="padding: 12px;">${booking.passengerCount}</td>
            <td style="padding: 12px;">₺${booking.totalPrice}</td>
            <td style="padding: 12px;">
                <span class="booking-status status-${booking.status.toLowerCase()}">
                    ${statusText}
                </span>
            </td>
            <td style="padding: 12px;">${booking.isPaid ? '✅' : '❌'}</td>
            <td style="padding: 12px;">
                ${booking.status === 'Confirmed' ? 
                    `<button class="btn btn-danger" onclick="bookingManager.cancelBooking(${booking.id})" style="padding: 6px 12px;">İptal Et</button>` : 
                    ''
                }
            </td>
        `;
        return row;
    }

    getStatusText(status, isPaid) {
        if (status === 'Cancelled') return 'İptal Edildi';
        if (status === 'Confirmed' && isPaid) return 'Ödendi';
        if (status === 'Confirmed' && !isPaid) return 'Onaylandı';
        return status;
    }

    async cancelBooking(id) {
        if (!confirm('⚠️ Rezervasyonu iptal etmek istediğinizden emin misiniz?')) return;

        try {
            await AdminAPI.cancelBooking(id);
            this.showAlert('Rezervasyon başarıyla iptal edildi', 'success');
            this.loadBookings();
            if (window.flightManager) {
                window.flightManager.loadFlights();
            }
        } catch (error) {
            this.showAlert(error.message, 'error');
        }
    }

    async deleteAllBookings() {
        const confirmMessage = `⚠️ DİKKAT: Tüm rezervasyonları silmek istediğinizden emin misiniz?\n\n` +
                              `• Toplam ${this.bookings.length} rezervasyon silinecek\n` +
                              `• Bu işlem geri alınamaz\n` +
                              `• Uçuşlardaki koltuk sayıları sıfırlanacak\n\n` +
                              `Devam etmek için "EVET" yazın:`;

        const userInput = prompt(confirmMessage);
        
        if (userInput !== 'EVET') {
            this.showAlert('İşlem iptal edildi', 'info');
            return;
        }

        try {
            const result = await AdminAPI.deleteAllBookings();
            this.showAlert(`Toplu silme tamamlandı!\n\n• ${result.deletedCount} rezervasyon silindi\n• ${result.deletedPassengers} yolcu kaydı silindi\n• Uçuş koltuk sayıları güncellendi`, 'success');
            this.loadBookings();
            if (window.flightManager) {
                window.flightManager.loadFlights();
            }
        } catch (error) {
            this.showAlert(error.message, 'error');
        }
    }

    showAlert(message, type = 'info') {
        alert((type === 'success' ? '✅ ' : type === 'error' ? '❌ ' : 'ℹ️ ') + message);
    }
}

// User management utilities
class UserManager {
    constructor() {
        this.users = [];
    }

    async loadUsers() {
        try {
            this.users = await AdminAPI.getUsers();
            this.renderUsers();
            this.updateStats();
        } catch (error) {
            console.error('Kullanıcılar yüklenemedi:', error);
            this.showAlert('Kullanıcılar yüklenirken hata oluştu', 'error');
        }
    }

    renderUsers() {
        const tbody = document.getElementById('users-tbody');
        if (!tbody) return;

        tbody.innerHTML = '';

        this.users.forEach(user => {
            const row = this.createUserRow(user);
            tbody.appendChild(row);
        });
    }

    createUserRow(user) {
        const row = document.createElement('tr');
        row.style.borderBottom = '1px solid #e0e0e0';
        row.innerHTML = `
            <td style="padding: 12px;">
                <div style="font-weight: bold;">
                    ${user.firstName && user.lastName ? `${user.firstName} ${user.lastName}` : 'Belirtilmemiş'}
                </div>
                ${user.identityNumber ? `<div style="font-size: 12px; color: #666;">TC: ${user.identityNumber}</div>` : ''}
            </td>
            <td style="padding: 12px;">${user.email}</td>
            <td style="padding: 12px;">${user.phoneNumber || '-'}</td>
            <td style="padding: 12px; text-align: center;">
                <span style="padding: 4px 8px; border-radius: 12px; font-size: 12px; font-weight: bold; background: ${user.isAdmin ? '#f44336' : user.isGuest ? '#9c27b0' : '#2196f3'}; color: white;">
                    ${user.isAdmin ? '👑 Admin' : user.isGuest ? '👤 Misafir' : '👨‍💼 Üye'}
                </span>
            </td>
            <td style="padding: 12px; text-align: center;">
                <span style="padding: 4px 8px; border-radius: 12px; font-size: 12px; font-weight: bold; background: ${user.isActive ? '#4caf50' : '#ff9800'}; color: white;">
                    ${user.isActive ? '✅ Aktif' : '⏸️ Pasif'}
                </span>
            </td>
            <td style="padding: 12px; text-align: center;">
                <span style="padding: 4px 8px; border-radius: 12px; font-size: 12px; font-weight: bold; background: ${user.bookingCount > 0 ? '#ff9800' : '#4caf50'}; color: white;">
                    ${user.bookingCount || 0} Rezervasyon
                </span>
            </td>
            <td style="padding: 12px; text-align: center; font-size: 12px; color: #666;">
                ${new Date(user.createdAt).toLocaleDateString('tr-TR')}
            </td>
            <td style="padding: 12px; text-align: center;">
                <div style="display: flex; gap: 8px; justify-content: center; flex-wrap: wrap;">
                    <button class="btn btn-sm" onclick="userManager.editUser(${user.id})" style="background: #ff9800; color: white; font-size: 12px; padding: 4px 8px;">
                        ✏️ Düzenle
                    </button>
                    ${!user.isGuest ? `
                        <button class="btn btn-sm" onclick="userManager.toggleUserStatus(${user.id})" style="background: ${user.isActive ? '#f44336' : '#4caf50'}; color: white; font-size: 12px; padding: 4px 8px;">
                            ${user.isActive ? '⏸️ Pasifleştir' : '▶️ Aktifleştir'}
                        </button>
                    ` : ''}
                    <button class="btn btn-sm" onclick="userManager.deleteUser(${user.id}, ${user.bookingCount || 0})" style="background: #f44336; color: white; font-size: 12px; padding: 4px 8px;">
                        🗑️ Sil
                    </button>
                </div>
            </td>
        `;
        return row;
    }

    updateStats() {
        const totalUsers = document.getElementById('total-users');
        const activeUsers = document.getElementById('active-users');
        const passiveUsers = document.getElementById('passive-users');
        const guestUsers = document.getElementById('guest-users');
        const adminUsers = document.getElementById('admin-users');
        const newUsers = document.getElementById('new-users');

        if (totalUsers) totalUsers.textContent = this.users.length;
        if (activeUsers) activeUsers.textContent = this.users.filter(u => u.isActive).length;
        if (passiveUsers) passiveUsers.textContent = this.users.filter(u => !u.isActive).length;
        if (guestUsers) guestUsers.textContent = this.users.filter(u => u.isGuest).length;
        if (adminUsers) adminUsers.textContent = this.users.filter(u => u.isAdmin).length;
        
        if (newUsers) {
            const oneMonthAgo = new Date();
            oneMonthAgo.setMonth(oneMonthAgo.getMonth() - 1);
            newUsers.textContent = this.users.filter(u => new Date(u.createdAt) >= oneMonthAgo).length;
        }
    }

    async createUser(userData) {
        try {
            await AdminAPI.createUser(userData);
            this.showAlert('Kullanıcı başarıyla oluşturuldu', 'success');
            this.loadUsers();
        } catch (error) {
            this.showAlert(error.message, 'error');
        }
    }

    async updateUser(id, userData) {
        try {
            await AdminAPI.updateUser(id, userData);
            this.showAlert('Kullanıcı başarıyla güncellendi', 'success');
            this.loadUsers();
        } catch (error) {
            this.showAlert(error.message, 'error');
        }
    }

    async toggleUserStatus(id) {
        if (!confirm('Kullanıcının durumunu değiştirmek istediğinizden emin misiniz?')) return;

        try {
            const result = await AdminAPI.toggleUserStatus(id);
            this.showAlert(result.message + (result.tempPassword ? `\n🔑 Geçici şifre: ${result.tempPassword}` : ''), 'success');
            this.loadUsers();
        } catch (error) {
            this.showAlert(error.message, 'error');
        }
    }

    async deleteUser(id, bookingCount) {
        const message = bookingCount > 0 
            ? `Bu kullanıcıyı ve ${bookingCount} rezervasyonunu silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.`
            : 'Bu kullanıcıyı silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.';
        
        if (!confirm(message)) return;

        try {
            await AdminAPI.deleteUser(id);
            this.showAlert('Kullanıcı başarıyla silindi', 'success');
            this.loadUsers();
        } catch (error) {
            this.showAlert(error.message, 'error');
        }
    }

    editUser(id) {
        const user = this.users.find(u => u.id === id);
        if (!user) return;

        // Populate edit form
        const editModal = document.getElementById('edit-modal');
        if (editModal) {
            document.getElementById('edit-userId').value = user.id;
            document.getElementById('edit-firstName').value = user.firstName || '';
            document.getElementById('edit-lastName').value = user.lastName || '';
            document.getElementById('edit-email').value = user.email;
            document.getElementById('edit-phoneNumber').value = user.phoneNumber || '';
            document.getElementById('edit-identityNumber').value = user.identityNumber || '';
            document.getElementById('edit-dateOfBirth').value = user.dateOfBirth ? user.dateOfBirth.split('T')[0] : '';
            document.getElementById('edit-gender').value = user.gender || 'Erkek';

            editModal.style.display = 'flex';
        }
    }

    showAlert(message, type = 'info') {
        alert((type === 'success' ? '✅ ' : type === 'error' ? '❌ ' : 'ℹ️ ') + message);
    }
}

// Global instances
let flightManager, bookingManager, userManager;

// Initialize admin functionality
document.addEventListener('DOMContentLoaded', function() {
    // Initialize managers
    flightManager = new FlightManager();
    bookingManager = new BookingManager();
    userManager = new UserManager();

    // Make them globally available
    window.flightManager = flightManager;
    window.bookingManager = bookingManager;
    window.userManager = userManager;

    // Initialize based on current page
    const currentPath = window.location.pathname;
    
    if (currentPath.includes('/Admin/Index') || currentPath.includes('/Admin')) {
        flightManager.loadFlights();
        bookingManager.loadBookings();
    } else if (currentPath.includes('/Admin/Users')) {
        userManager.loadUsers();
    }
});

// Global utility functions for admin
window.toggleFlightForm = function() {
    if (window.flightManager) {
        const form = document.getElementById('flight-form');
        if (form.style.display === 'none' || !form.style.display) {
            window.flightManager.showFlightForm();
        } else {
            window.flightManager.hideFlightForm();
        }
    }
};

window.cancelFlightForm = function() {
    if (window.flightManager) {
        window.flightManager.hideFlightForm();
    }
};

window.deleteAllBookings = function() {
    if (window.bookingManager) {
        window.bookingManager.deleteAllBookings();
    }
};

window.showCreateModal = function() {
    const modal = document.getElementById('create-modal');
    if (modal) modal.style.display = 'flex';
};

window.hideCreateModal = function() {
    const modal = document.getElementById('create-modal');
    if (modal) {
        modal.style.display = 'none';
        document.getElementById('create-form').reset();
    }
};

window.showEditModal = function() {
    const modal = document.getElementById('edit-modal');
    if (modal) modal.style.display = 'flex';
};

window.hideEditModal = function() {
    const modal = document.getElementById('edit-modal');
    if (modal) {
        modal.style.display = 'none';
        document.getElementById('edit-form').reset();
    }
};