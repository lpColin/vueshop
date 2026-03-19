import { getToken, logout } from './auth'

const BASE_URL = 'http://192.168.1.21:5162'

function getCurrentRoute() {
  const pages = getCurrentPages()
  const current = pages[pages.length - 1]
  return current?.route || ''
}

function redirectToLogin() {
  if (getCurrentRoute() === 'pages/login/login') return
  uni.reLaunch({ url: '/pages/login/login' })
}

function redirectToUserCenter() {
  if (getCurrentRoute() === 'pages/user/user') return
  uni.switchTab({ url: '/pages/user/user' })
}

function request(options = {}) {
  const token = getToken()
  const headers = {
    'Content-Type': 'application/json',
    ...(options.header || {})
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  return new Promise((resolve, reject) => {
    uni.request({
      url: `${BASE_URL}${options.url || ''}`,
      method: options.method || 'GET',
      data: options.data || {},
      header: headers,
      success: (res) => {
        if (res.statusCode === 401) {
          logout()
          uni.showToast({ title: '登录已失效，请重新登录', icon: 'none' })
          setTimeout(() => {
            redirectToLogin()
          }, 300)
          reject(new Error('UNAUTHORIZED'))
          return
        }

        if (res.statusCode === 403) {
          uni.showToast({ title: '无权限访问后台页面', icon: 'none' })
          setTimeout(() => {
            redirectToUserCenter()
          }, 300)
          reject(new Error('FORBIDDEN'))
          return
        }

        if (res.statusCode >= 200 && res.statusCode < 300) {
          const body = res.data || {}
          if (body.success === false) {
            reject(new Error(body.message || '请求失败'))
            return
          }
          resolve(body)
          return
        }

        reject(new Error((res.data && res.data.message) || `HTTP_${res.statusCode}`))
      },
      fail: (err) => reject(err)
    })
  })
}

export default request
