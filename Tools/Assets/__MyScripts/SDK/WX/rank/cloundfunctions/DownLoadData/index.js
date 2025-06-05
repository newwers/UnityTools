// 云函数入口文件
const cloud = require('wx-server-sdk')

cloud.init({ env: cloud.DYNAMIC_CURRENT_ENV }) // 使用当前云环境

//获取数据库引用
const db = cloud.database() ;
const UserData = db.collection('db');

// 云函数入口函数
exports.main = async (event, context) => {
  try{
    //const {level} = event;
    const levelField = 'gamedata.level';
    //初始化查询条件
    let query = UserData.where({
      'gamedata.avatarUrl':db.command.neq(""),//确保avatar不为空
      'gamedata.nickName':db.command.neq(""),//确保nick不为空
    });
    //使用构造除的字段名进行查询和排序,限制返回的结果数量为50
    let data = await query.orderBy(levelField,'desc').limit(100).get();
    //检查获取的数据是否为空
    if(data.length == 0){
      return{
        code:0,//自定义错误状态码
        msg:'No found data'
      };
    }
    else{
      return{
        code:1,
        data:data.data//返回获取到的数据
      };
    }
  }
  catch(error){
    return{
      code:-1,//抛出异常
      error:error.message
    };
  }
}