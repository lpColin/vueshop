const TOKEN_KEY = 'token'
const USER_KEY = 'userInfo'

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
  return uni.getStorageSync(USER_KEY) || null
}

export function setUserInfo(user) {
  uni.setStorageSync(USER_KEY, user || null)
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
