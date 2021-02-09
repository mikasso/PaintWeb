function Point(x, y) {
    this.x = x;
    this.y = y;
}

function Line(p1 = null, p2 = null, color = "green") {
    this.p1 = p1;
    this.p2 = p2;
    this.color = color;
}

function CanvasManager(canvas) {
    this.canvas = canvas;
    this.HTMLelement = canvas[0];
    this.ctx = this.HTMLelement.getContext("2d");
    this.paint = false;
    this.line = new Line(0, 0);
    this.hub = {};

    this.drawLine = function (line) {
        this.ctx.strokeStyle = line.color;
        this.ctx.lineJoin = "round";
        this.ctx.lineWidth = 2;
        this.ctx.beginPath();
        this.ctx.moveTo(line.p1.x, line.p1.y);
        this.ctx.lineTo(line.p2.x, line.p2.y);
        this.ctx.closePath();
        this.ctx.stroke();
    };

    this.getOffset = function () {
        const rect = this.HTMLelement.getBoundingClientRect();
        return {
            left: rect.left + window.scrollX,
            top: rect.top + window.scrollY
        };
    };

    this.getPointFromEvent = function (event) {
        const offset = this.getOffset();
        return new Point(
            event.pageX - offset.left,
            event.pageY - offset.top);
    };

    this.canvas.on('mousedown', () => {
        console.log(event);
        this.line.p1 = this.getPointFromEvent(event);
        this.paint = true;
    });

    this.canvas.on('mouseup', () => {
        this.paint = false
        console.log("Mouse up");
    });

    this.canvas.on('mousemove', (event) => {
        if (this.paint == false)
            return;
        this.line.p2 = this.getPointFromEvent(event);
        if (this.hub != null)
            if (this.hub.isConnected == true) {
                this.drawLine(this.line);
                this.hub.sendLine(this.line);
            }
        this.line.p1 = this.line.p2;
    });
}