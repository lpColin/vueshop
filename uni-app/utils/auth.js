const TOKEN_KEY = 'token'
const USER_KEY = 'userInfo'

export function normalizeUserInfo(user) {
  if (!user) return null
  return {
    ...user,
    id: user.id ?? user.Id ?? '',
    username: user.username ?? user.Username ?? '',
    nickname: user.nickname ?? user.Nickname ?? '',
    avatar: user.avatar ?? user.Avatar ?? '',
    phone: user.phone ?? user.Phone ?? '',
    role: user.role ?? user.Role ?? 'user',
    shopId: user.shopId ?? user.ShopId ?? null
  }
}

export function getToken() {
  return uni.getStorageSync(TOKEN_KEY) || ''
}

export function setToken(token) {
  uni.setStorageSync(TOKEN_KEY, token || '')
}

export function clearToken() {
  uni.removeStorageSync(TOKEN_KEY)
}

export function getUserInfo() {
  return normalizeUserInfo(uni.getStorageSync(USER_KEY) || null)
}

export function setUserInfo(user) {
  uni.setStorageSync(USER_KEY, normalizeUserInfo(user))
}

export function clearUserInfo() {
  uni.removeStorageSync(USER_KEY)
}

export function isLoggedIn() {
  return Boolean(getToken())
}

export function logout() {
  clearToken()
  clearUserInfo()
}
