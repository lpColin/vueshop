// 订单详情页面
const app = getApp()

Page({
  data: {
    orderId: 0,
    order: {}
  },

  onLoad: function(options) {
    const orderId = parseInt(options.id) || 0
    this.setData({ orderId })
    this.loadOrderDetail(orderId)
  },

  loadOrderDetail: function(id) {
    const orders = wx.getStorageSync('orders') || []
    let order = orders.find(o => o.id === id)
    
    if (order) {
      // 格式化时间
      const date = new Date(order.createTime)
      const createTimeStr = date.getFullYear() + '-' + 
        String(date.getMonth() + 1).padStart(2, '0') + '-' + 
        String(date.getDate()).padStart(2, '0') + ' ' + 
        String(date.getHours()).padStart(2, '0') + ':' + 
        String(date.getMinutes()).padStart(2, '0')
      
      // 生成订单号
      const orderNo = 'SO' + Date.now()
      
      // 计算商品总额
      const goodsTotal = order.items.reduce((sum, item) => {
        return sum + item.price * item.quantity
      }, 0)
      
      this.setData({
        order: {
          ...order,
          createTimeStr,
          orderNo,
          goodsTotal: goodsTotal.toFixed(2)
        }
      })
    } else {
      // 如果没有找到订单，创建一个示例订单用于展示
      const sampleOrder = {
        id: id,
        status: 1,
        items: [
          { id: 1, name: '红富士苹果 5 斤装', price: 39.00, quantity: 2, image: 'https://picsum.photos/seed/apple/300/300' },
          { id: 2, name: '进口香蕉 1 把', price: 15.80, quantity: 3, image: 'https://picsum.photos/seed/banana/300/300' }
        ],
        total: 125.40,
        address: {
          name: '张三',
          phone: '138****8888',
          detail: '北京市朝阳区 xxx 街道 xxx 小区 xxx 号楼 xxx 室'
        },
        createTime: new Date().toISOString(),
        createTimeStr: new Date().toLocaleString('zh-CN'),
        orderNo: 'SO' + Date.now(),
        goodsTotal: '125.40'
      }
      
      this.setData({ order: sampleOrder })
    }
  },

  payOrder: function() {
    wx.showLoading({ title: '正在支付...' })
    setTimeout(() => {
      wx.hideLoading()
      this.updateOrderStatus(1)
      wx.showToast({ title: '支付成功', icon: 'success' })
    }, 1500)
  },

  cancelOrder: function() {
    const that = this
    wx.showModal({
      title: '提示',
      content: '确定要取消订单吗？',
      success: function(res) {
        if (res.confirm) {
          that.updateOrderStatus(4)
          wx.showToast({ title: '订单已取消', icon: 'success' })
        }
      }
    })
  },

  confirmReceive: function() {
    const that = this
    wx.showModal({
      title: '提示',
      content: '确认已收到商品吗？',
      success: function(res) {
        if (res.confirm) {
          that.updateOrderStatus(3)
          wx.showToast({ title: '已确认收货', icon: 'success' })
        }
      }
    })
  },

  updateOrderStatus: function(status) {
    const { orderId } = this.data
    const orders = wx.getStorageSync('orders') || []
    const orderIndex = orders.findIndex(o => o.id === orderId)
    
    if (orderIndex >= 0) {
      orders[orderIndex].status = status
      wx.setStorageSync('orders', orders)
      this.loadOrderDetail(orderId)
    } else {
      // 如果是示例订单，只更新本地数据
      this.data.order.status = status
      this.setData({ order: this.data.order })
    }
  }
})        that.updateOrderStatus(4)
          wx.showToast({ title: '订单已取消', icon: 'success' })
        }
      }
    })
  },

  confirmReceive: function() {
    const that = this
    wx.showModal({
      title: '提示',
      content: '确认已收到商品吗？',
      success: function(res) {
        if (res.confirm) {
          that.updateOrderStatus(3)
          wx.showToast({ title: '已确认收货', icon: 'success' })
        }
      }
    })
  },

  updateOrderStatus: function(status) {
    const { orderId } = this.data
    const orders = wx.getStorageSync('orders') || []
    const orderIndex = orders.findIndex(o => o.id === orderId)
    
    if (orderIndex >= 0) {
      orders[orderIndex].status = status
      wx.setStorageSync('orders', orders)
      this.loadOrderDetail(orderId)
    } else {
      // 如果是示例订单，只更新本地数据
      this.data.order.status = status
      this.setData({ order: this.data.order })
    }
  }
