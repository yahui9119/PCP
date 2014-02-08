
/**
 * Module dependencies.
 */

var express = require('express');

var mongoose = require('mongoose');

var ejs=require('ejs');//模板引擎

var less=require('less-middleware');

var routes = require('./controller');

var http = require('http');//http服务

var path = require('path');

var app = express();

// all environments
app.set('port', process.env.PORT || 3000);
app.set('views', path.join(__dirname, 'views'));
app.engine('html',ejs.__express);//使用html后缀
app.set('view engine', 'html');
app.use(express.favicon());
app.use(express.logger('dev'));
app.use(express.json());
app.use(express.urlencoded());
app.use(express.methodOverride());
app.use(express.cookieParser('your secret here'));
app.use(express.session());
app.use(app.router);
app.use(less({ src: path.join(__dirname, 'public') }));
app.use(express.static(path.join(__dirname, 'public')));

// development only
if ('development' == app.get('env')) {
  app.use(express.errorHandler());
}
var user = require('./controller/user');
var pcp=require('./controller/pcp');
app.get('/', routes.index);
app.get('/users', user.list);
//pcp
app.get('/pcp',pcp.index);
app.get('/pcp/login',pcp.login);
app.get('/pcp/regist',pcp.regist);
app.get('/pcp/test',pcp.test);
http.createServer(app).listen(app.get('port'), function(){
  console.log('Express server listening on port ' + app.get('port'));
});

mongoose.connect('mongodb://localhost/db');