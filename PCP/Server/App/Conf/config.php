<?php
//注意，请不要在这里配置SAE的数据库，配置你本地的数据库就可以了。
return array(
    //'配置项'=>'配置值'
    'DEFAULT_MODULE'     => 'Index', //默认模块
    'URL_MODEL'          => '1', //URL模式
    'SESSION_AUTO_START' => true, //是否开启session
    //'SHOW_PAGE_TRACE'=>true,
    'URL_HTML_SUFFIX'=>'.html',
    // 添加数据库配置信息
 'DB_TYPE'   => 'mysql', // 数据库类型
 'DB_HOST'   => 'localhost', // 服务器地址
 'DB_NAME'   => 'app_servershost', // 数据库名
 'DB_USER'   => 'root', // 用户名
 'DB_PWD'    => '', // 密码
 'DB_PORT'   => 3306, // 端口
 'DB_PREFIX' => 's_', // 数据库表前缀
    
);
?>