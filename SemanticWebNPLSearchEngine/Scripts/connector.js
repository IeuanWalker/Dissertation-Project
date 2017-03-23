//https://codepen.io/florian/details/gAxtK

$(document).ready(function(){ 
    dotGenerator(0, true);
    dotGenerator(1, false);
});

function dotGenerator(i, linesOrNot) {
    var canvas = $("canvas")[i];

    canvas.style.width = '100%';
    canvas.style.height = '100%';
    canvas.width = canvas.offsetWidth;
    canvas.height = canvas.offsetHeight;
    canvas.style.display = 'block';
    
    var ctx = canvas.getContext("2d");
    ctx.lineWidth = .2;

    // Generate a color
    function Color(min) {
        min = min || 0;
        this.r = this.value(min);
        this.g = this.value(min);
        this.b = this.value(min);
        this.style = "rgba( 0, "+ this.g +","+ this.b +",0.3)";
    }
  
    Color.prototype = {
        value: function(min) {
            return Math.floor(Math.random()*255 + min);
        }
    }
    
    function mixComponents(comp1, comp2, weight1, weight2) {
        return (comp1*weight1 + comp2*weight2) / (weight1 + weight2);
    }   
    
    function gradient(dot1, dot2, midColor) {
        var grad = ctx.createLinearGradient(Math.floor(dot1.x), Math.floor(dot1.y), Math.floor(dot2.x), Math.floor(dot2.y));
        grad.addColorStop(0, dot1.color.style);
        grad.addColorStop(Math.floor(dot1.radius / (dot1.radius/dot2.radius)), midColor);
        grad.addColorStop(1, dot2.color.style);
        return grad;
    }
  
    function lineStyle(dot1, dot2) {
        var r = mixComponents(dot1.color.r, dot2.color.r, dot1.radius, dot2.radius);
        var g = mixComponents(dot1.color.g, dot2.color.g, dot1.radius, dot2.radius);;
        var b = mixComponents(dot1.color.b, dot2.color.b, dot1.radius, dot2.radius);;
        var midColor = 'rgba(' + Math.floor(r) + ',' + Math.floor(g) + ',' + Math.floor(b) + ', 0.8)';

        return gradient(dot1, dot2, midColor);
    }
    
    var connectArea = {
        x: 50*canvas.width / 100,
        y: 50*canvas.height / 100
    };

    var numberOfDots = 0;
    numberOfDots = (canvas.width * canvas.height) / 5300;

    var dots = {
        nb: numberOfDots,
        distMax: 80,
        connectAreaRadius: 100,
        array: []
    };
  
    function Dot() {
        this.x = Math.random() * canvas.width;
        this.y = Math.random() * canvas.height;
        this.vx = Math.random() - 0.5;
        this.vy = Math.random() - 0.5;  
        this.radius = Math.random() * 2;
        this.color = new Color();
    }
  
    Dot.prototype = {
        draw: function() {
            ctx.beginPath();
            ctx.fillStyle = this.color.style;
            ctx.arc(this.x, this.y, this.radius, 0, 2*Math.PI, false);
            ctx.fill();
        }
    };
  
    function moveDots() {
        for (var i = 0; i < dots.nb; i++) {
            var dot = dots.array[i];

            if (dot.y < 0 || dot.y > canvas.height) {
                dot.vy = - dot.vy;
            } else if(dot.x < 0 || dot.x > canvas.width) {
                dot.vx = - dot.vx;
            }
            dot.x += dot.vx;
            dot.y += dot.vy;
        }
    }
    
    //If u dont want to connect the lines
    if(linesOrNot){
        function connectDots() {
            for (var i = 0; i < dots.nb; i++) {
                for (var j = 0; j < dots.nb; j++) {
                    if (i === j) continue;

                    var dot1 = dots.array[i];
                    var dot2 = dots.array[j];

                    var xDiff = dot1.x - dot2.x;
                    var yDiff = dot1.y - dot2.y;
                    var xCoreDiff = dot1.x - connectArea.x;
                    var yCoreDiff = dot1.y - connectArea.y;

                    if ( (xDiff < dots.distMax && xDiff > -dots.distMax) 
                        && (yDiff < dots.distMax && yDiff > -dots.distMax) 
                        && (xCoreDiff < dots.connectAreaRadius && xCoreDiff > -dots.connectAreaRadius)
                        && (yCoreDiff < dots.connectAreaRadius && yCoreDiff > -dots.connectAreaRadius) ) {
                        ctx.beginPath();
                        ctx.strokeStyle = lineStyle(dot1, dot2);
                        ctx.moveTo(dot1.x, dot1.y);
                        ctx.lineTo(dot2.x, dot2.y);
                        ctx.stroke();
                        ctx.closePath();
                    }
                }
            }
        }
        $("canvas").on("mousemove", function(e) {
            connectArea.x = e.pageX;
            connectArea.y = e.pageY;
        });
    }
  
    function createDots() {
        for(i = 0; i < dots.nb; i++) {
            dots.array.push(new Dot());
        }
    }
  
    function drawDots() {
        for(i = 0; i < dots.nb; i++) {
            dot = dots.array[i];
            dot.draw();
        }
    }
  
    function animateDots() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        moveDots();
        if(linesOrNot){
            connectDots();
        }
        drawDots();
    }

    

    createDots();
    setInterval(animateDots, 40);
  
}