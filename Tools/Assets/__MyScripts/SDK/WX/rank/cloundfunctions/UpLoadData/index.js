// 云函数入口文件
const cloud = require('wx-server-sdk')

cloud.init({ env: cloud.DYNAMIC_CURRENT_ENV }) // 使用当前云环境

//获取数据库引用
const db = cloud.database() ;
const gamedata = db.collection('db');

// 云函数入口函数
exports.main = async (event, context) => {
  const wxContext = cloud.getWXContext()
  //查询用户是否已经保存过数据
  let _isHas = await gamedata.where({
    openid :wxContext.OPENID
  }).get();
  //如果没有,首次保存
  if(_isHas.data.length==0){
    let _isAdd = null ;
    let addData ={
      openid:wxContext.OPENID,
      gamedata:event,
    }
    _isAdd = await gamedata.add({
      data:addData
    })
    return{
      code:0,
      res:_isAdd,
      data:addData,
    };
  }
  else{
    return await gamedata.where({
      openid:wxContext.OPENID
    }).update({
      data:{
        openid:wxContext.OPENID,
        gamedata:event,
      }
    })
  }
}