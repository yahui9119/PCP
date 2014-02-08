<?php
function show_db_errorxx(){
	exit('系统访问量大，请稍等添加数据');
}
///获取客户端的ip信息
function GetClientIp()
{
    //$ip= $_SERVER['REMOTE_ADDR'];
    //$ip=$ip=='::1'?'127.0.0.1':$ip;
    $ip=get_client_ip();
    $ip=$ip=='0.0.0.0'?'127.0.0.1':$ip;
    return $ip;
}
///获取客户端的端口号
function GetClientPort()
{
    return $_SERVER['REMOTE_PORT'];
}


?>