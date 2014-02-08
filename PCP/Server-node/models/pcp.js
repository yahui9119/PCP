var mongoose = require('mongoose');
var Schema = mongoose.Schema
  , ObjectId = Schema.ObjectId;
  var pcpUser=new Schema({
      id:ObjectId
     ,account:{type:String}
     ,password:{type:String}
     ,ClientIP:{type:String}
     ,ClientPort:{type:Number,min:0,max:65535}
     ,date:{type:Date,default:Date.now}
     });
exports.pcpUser=mongoose.model("pcpUser",pcpUser);//pcp用户