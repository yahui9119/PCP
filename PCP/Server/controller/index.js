
/*
 * GET home page.
 */

exports.index = function(req, res){
    var point=req.socket._peername.port;
  res.render('index', { title: 'Express' ,point:point});
};