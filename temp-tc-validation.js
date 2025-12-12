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

// Test
console.log(validateTCKimlik('12345678901')) // false - geçersiz
console.log(validateTCKimlik('11111111111')) // false - tüm haneler aynı
console.log(validateTCKimlik('01234567890')) // false - 0 ile başlıyor
console.log(validateTCKimlik('12345678902')) // Geçerli bir TC olup olmadığını test eder