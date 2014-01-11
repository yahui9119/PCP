
/*
 * GET users listing.
 */

exports.index = function(req, res){
  res.send("pcp首页");
};
exports.login = function(req, res){
  res.send("pcp登录");
};
exports.regist = function(req, res){
  res.send("pcp注册");
};
exports.test = function(req, res){
   res.render('test', { title: 'test' });
   
};