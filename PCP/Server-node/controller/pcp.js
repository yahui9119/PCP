
/*
 * GET users listing.
 */
var pcpmodel=require('../models/pcp');
var User=pcpmodel.pcpUser;

exports.index = function(req, res){
  res.render("pcp/index.html",{title:"这里是首页，功能还在测试时间！敬请期待！"});
};
exports.login = function(req, res){
   
  res.render("pcp/login.html",{title:'登录页，请输入用户名和密码'});
};
exports.regist = function(req, res){
    var user=new User({
        account:'testaccount'
     ,password:'123456'
     ,ClientIP:'127.0.0.1'
     ,ClientPort:8080
        });
  var i= user.save();
   res.render("pcp/register.html",{title:'登录页，请输入用户名和密码'});
};
exports.test = function(req, res){
  res.render("pcp/test.html",{title:'登录页，请输入用户名和密码'});
};