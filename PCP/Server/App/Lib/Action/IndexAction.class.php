<?php


//欢迎页面
 class IndexAction extends Action {
      protected function _initialize() {
        header("Content-Type:text/html; charset=utf-8");
    }
    public function Index(){
        $assign= array();
        $assign['ip']= $_SERVER['REMOTE_ADDR'];
        $assign['point']= $_SERVER['REMOTE_PORT'];
        $this->assign($assign);
        $this->display();

    }
}