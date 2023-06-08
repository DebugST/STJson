$(document).ready(function(){
    let strModel = $("#div_model").text()
    if(strModel == "BOOK"){
        $("#div_left").remove();
        $("#div_logo").css({"left":0});
        $("#div_right").css({"margin-left":location.search.indexOf("?iframe") == -1 ? 10 : 0});
    }else{
        $("#div_right_list").remove();
        if(strModel == "HOME"){
            loadHome();
        }
    }
    $(".anchor_btn,#div_right_list a").click(function(){
        scrollToAnchor($(this).attr('name'));
    });
    $("#div_right").scroll(function(){
        var nMin = 100000,strName = '',strLast = '';
        var nHeight =window.innerHeight;// document.body.clientHeight;
        var nHtmlTop = $(this).scrollTop();
        var es = $('.anchor_point');
        for(i = 0; i < es.length; i++){
            var nSub = Math.abs(es[i].offsetTop - nHtmlTop);
            if(nSub < nMin){
                nMin = nSub;
                if(nHtmlTop + (nHeight / 2) >= es[i].offsetTop){
                    strName = $(es[i]).attr('name');
                }
            }
            strLast = $(es[i]).attr('name');
        }
        $(".anchor_btn").removeClass('active');
        $(".anchor_btn[name='" + strName + "']").addClass('active');
        $("#div_right_list a").removeClass('active');
        $("#div_right_list a[name='" + strName + "']").addClass('active');
    });
    
    function loadHome(){
        $(".a_node_root").click(function(){
            $(this).next().toggle(500);
        }).css({
            "line-height":"29px",
            "border-top":"1px solid black"
        });
        $("#div_right").css({"margin-right":0,"overflow":"hidden"});
        let search = location.search;
        let m = search.match("id=([0-9.]+)");
        if(!m) {
            $('iframe').attr("src","./pages/introduction.html?iframe" + location.hash);
            return;
        };
        $('iframe').attr("src","./pages/" + m[1] + ".html?iframe" + location.hash);
        let strs = m[1].split('.');
        let ele =$($(".a_node_root")[parseInt(strs[0]) - 1]);
        ele.css({"background-color":"#0076A0","color":"white"})
        .next().show();
        $(ele.next().find("a")[parseInt(strs[1]) - 1]).css({"background-color":"rgba(0, 118, 160,.5)","color":"white"});
        $("#div_left_list").scrollTop(ele.offset().top);
    }
    
    function scrollToAnchor(strName){
        var nTop = $(".anchor_point[name='" + strName + "']").offset().top;
        nTop = $('#div_right').scrollTop() + nTop - 5;
        $('#div_right').animate({scrollTop:nTop},500,function(){
            parent.window.location.hash = location.hash;
        });
    }
    
    $(document).on("touchstart","#div_left",function(e){});
    
    if(strModel != "HOME"){
        var hash = window.location.hash;
        if(hash && hash[0] == '#'){
            scrollToAnchor(hash.substring(1));
        }
    }
    
    var strUA = navigator.userAgent.toLowerCase();
    if(strUA.indexOf('windows') != -1 && strUA.indexOf('webkit') == -1){
        //console.log('what the fuck...!!!!!!');
        //我是真没想到都2021年了 windows还在采用可视化滚动条
        //而且还只有webkit内核提供了滚动条样式的支持
        //我一直以为现在这个年代 滚动条基本都是隐藏式了的吧
        //且不说windows 浏览器厂商就这么赤裸裸的使用系统原生滚动条真棒
        //我不是吐槽没有解决方案 而是这种设计
        //当发现问题后 我仅仅是想通过样式隐藏滚动条 而只有webkit能做到
        //::-webkit-scrollbar { display: none; }
        $('body').append(
            "<div id='WO_TE_ME_DE_YE_HEN_JUE_WANG_A_CAO'>"
            + "老铁!用WebKit浏览器,不然滚动条太丑了,不想改页面了！<br/>"
            + "(Use the WebKit browser as much as possible!!)"
            + "</div>"
        );
        $('#WO_TE_ME_DE_YE_HEN_JUE_WANG_A_CAO').css({
            position:"fixed",
            top:0,
            left:0,
            right:0,
            color:"white",
            "line-height":"20px",
            "text-align":"center",
            "background-color":"rgba(255,255,0,.6)",
            border:"solid 1px yellow",
            "text-shadow":"0px 1px 1px black",
            "z-index":100
        }).click(function(){$(this).remove();});
    }
});