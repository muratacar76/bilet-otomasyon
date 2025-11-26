-- Koltuk seçimi için yeni alanlar ekleniyor

-- Passengers tablosuna koltuk bilgileri
ALTER TABLE Passengers ADD SeatNumber NVARCHAR(10) NOT NULL DEFAULT '';
ALTER TABLE Passengers ADD SeatType NVARCHAR(20) NOT NULL DEFAULT '';

-- Flights tablosuna koltuk düzeni bilgileri
ALTER TABLE Flights ADD SeatsPerRow INT NOT NULL DEFAULT 6;
ALTER TABLE Flights ADD TotalRows INT NOT NULL DEFAULT 30;

PRINT 'Veritabanı başarıyla güncellendi - Koltuk seçimi özellikleri eklendi';
