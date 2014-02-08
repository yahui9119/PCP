<?php


//欢迎页面
 class PcpAction extends Action {
      protected function _initialize() {
        header("Content-Type:text/html; charset=utf-8");
    }
    public function Index(){
        $assign= array();
        $ip= $_SERVER['REMOTE_ADDR'];
        $ip=$ip=='::1'?'127.0.0.1':$ip;
        $assign['ip']=$ip;
        $assign['point']= $_SERVER['REMOTE_PORT'];
        $this->assign($assign);
        $data=M('pcp_member')->select();
        var_dump($data);
        $this->display();

    }
    public function Login(){
        
        $result['result']=false;
        $result['message']="用户名或密码错误";
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
            $result['message']="恭喜 登录成功";
        }
         echo json_encode($result);
    }

    public function Regist(){
        echo '请联系管理员';
    }
    /// 根据个人信息
    ///获取在线的个人账户服务器
    public function Online(){
        $member=session('member');
        if($member)
        {
            $assign['ip']= $_SERVER['REMOTE_ADDR'];
            $assign['point']= $_SERVER['REMOTE_PORT'];
            $this->assign($assign);
            $this->display();
        }
        else
        {
        	echo '请先登录';
        }
        
    }
}