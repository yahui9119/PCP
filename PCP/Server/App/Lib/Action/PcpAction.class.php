<?php


//欢迎页面
 class PcpAction extends Action {
      protected function _initialize() {
        header("Content-Type:text/html; charset=utf-8");
    }
    public function Index(){
        $assign= array();
        $assign['ip']=GetClientIp();
        $assign['point']= GetClientPort();
        $this->assign($assign);
        $data=M('pcp_member')->select();
        var_dump($data);
        //$this->ajaxReturn($result,"json");

    }
    public function Login(){
        
        $result['result']=false;
        $result['message']="username or password is error";
        $username=$this->_get('username');
        $password=$this->_get('password');
        $wherepara['username']=array('eq',$username);
        $wherepara['password']=array('eq',$password);//md5 not yet!
                                                     //vcode not yet!
        $data=M('pcp_member')->where($wherepara)->select();
        if($data){
            $member=array();
            $member['username']=$username;
            $member['ticket']=md5(date('Y-m-d H:i:s',time()));
            session('member',$member);
            $result['result']=true;
            $result['message']="login success";
        }
        $this->ajaxReturn($result,"json");
    }

    public function Regist(){
        echo '请联系管理员';
    }
    /// 根据个人信息
    ///获取在线的个人账户服务器
    public function Online(){
        $member=session('member');
        $result['result']=false;
        $result['message']="please login！";
        if($member)
        {
            //用户已经登录 添加记录
            $assign['ip']=GetClientIp();
            $assign['point']= GetClientPort();
            //添加服务主机在线记录
            
            $onlinedata=M('pcp_onlinelog');
            $onlinelog['ip']=GetClientIp();
            $onlinelog['port']=GetClientPort();
            $onlinelog['onlinekey']=md5(GetClientIp().$member['username']);//还未确定
            $onlinelog['time']=date('Y-m-d H:i:s',time());
            $onlinelog['mode']=1;
            $onlinelog['username']=$member['username'];
            if($onlinedata->create($onlinelog)){   
                $state= $onlinedata->add();
                if($state)
                {
                    sleep(10);//暂定延迟10秒
                    $result['result']=true;
                    $result['message']="add success";
                    $this->ajaxReturn($result,"json");
                   
                }
            }
            $result['result']=false;
            $result['message']="add fail";
        }
        else
        {
        	//搜寻服务主机
            $username=$this->_get('username');
            $wherepara['username']=array('eq',$username);
            //vcode not yet!
            $onlinedata=M('pcp_onlinelog')->where($wherepara)->order('time desc')->find();
            $result['message']=$onlinedata;
        }
        
        $this->ajaxReturn($result,"json");
    }
    static $ClientSocket=array();
    //查看服务器状体，也用于第一次启动
    public function ServerState( )
    {
        ignore_user_abort(true);//关掉浏览器，PHP脚本也可以继续执行.
        set_time_limit(0);// 通过set_time_limit(0)可以让程序无限制的执行下去
        $interval=60*30;// 每隔半小时运行
        //while (true)//无限执行下去
        //{
        //    sleep(1);//暂定延迟1秒
        //}
    }
    
}