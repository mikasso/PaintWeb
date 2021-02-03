"use strict";

class Point {
    constructor(x, y) {
        this.x = x;
        this.y = y;
    }
}

class Line {
    constructor(p1 = null, p2 = null, color = "green") {
        this.p1 = p1;
        this.p2 = p2;
        this.color = color;
    }
}

class CanvasManager {
    constructor(canvas) {
        this.canvas = canvas;
        this.HTMLelement = canvas[0];
        this.ctx = this.HTMLelement.getContext("2d");
    }

    drawLine(line) {
        this.ctx.strokeStyle = line.color;
        this.ctx.lineJoin = "round";
        this.ctx.lineWidth = 2;
        this.ctx.beginPath();
        this.ctx.moveTo(line.p1.x, line.p1.y);
        this.ctx.lineTo(line.p2.x, line.p2.y);
        this.ctx.closePath();
        this.ctx.stroke();
    }

    getOffset() {
        const rect = this.HTMLelement.getBoundingClientRect();
        return {
            left: rect.left + window.scrollX,
            top: rect.top + window.scrollY
        };
    }

    getPointFromEvent(event) {
        const offset = this.getOffset();
        return new Point(
            event.pageX - offset.left,
            event.pageY - offset.top);
    }


}


function sendLine(line) {
    console.log(JSON.stringify(line));
    $.ajax({
        url: "/Painting/PostPainting",
        type: "POST",
        data: JSON.stringify(line),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            console.log("succesed send!");
        },
        error: function () {
            console.log("Not send");
        }
    });
}

function getLines(ctx) {
    $.ajax({
        url: "/Painting/GetLine",
        type: "GET",
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (line) {
            drawLine(ctx, line.P1, line.P2);
            console.log("received line");
            getLines(ctx);
        },
        error: function () {
            console.log("Error");
        }
    });
}

function clearCanvas() {
    const canvas = $("#MainCanvas")[0];
    const context = canvas.getContext('2d');
    context.clearRect(0, 0, canvas.width, canvas.height);
}

$(function () {
    var jQueryCanvas = $("#MainCanvas");
    let canvasManager = new CanvasManager(jQueryCanvas);
    let line = new Line();
    var paint = false;
    var canvas = canvasManager.canvas;
    var sending = false;
    var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
    var user = "user";
    var group = "";
    canvas.on('mousedown', function () {
        console.log(event);
        line.p1 = canvasManager.getPointFromEvent(event);
        paint = true;
    });

    canvas.on('mouseup', function () {
        paint = false
        console.log("Mouse up");
        //sendLine(p1,p2)
    });

    canvas.on('mousemove', function () {
        if (paint == false)
            return;
        line.p2 = canvasManager.getPointFromEvent(event);
        canvasManager.drawLine(line);

        // send line
        if (sending == true) {
            connection.invoke("SendLineToGroup", user, JSON.stringify(line)).catch(function (err) {
                return console.error(err.toString());
            });
            event.preventDefault();
        }
        line.p1 = line.p2;
    });

    connection.on("ClearCanvas", function (user, message) {
        console.log("Clear");
        clearCanvas();
    });

    connection.on("ReceiveLine", function (user, message) {
        canvasManager.drawLine(JSON.parse(message));
    });

    connection.on("ReceiveText", function (user, message) {
        console.log(message);
    });

    window.onbeforeunload = function () {
        if (group != "")
            connection.invoke("RemoveFromGroup", user, group).catch(function (err) {
                alert(err.toString());
            })};

    document.getElementById("ConnectToGroupButton").addEventListener("click", function (event) {
        connection.start().then(function () {
            sending = true;
            group = document.getElementById("GroupTextInput").value;

            connection.invoke("AddToGroup", user, group).catch(function (err) {
                return console.error(err.toString());
            });

            connection.invoke("SynchronizeCanvas", user, "synchronizecanvas").catch(function (err) {
                return console.error(err.toString());
            });

            document.getElementById("ClearCanvasButton").addEventListener("click", function (event) {
                var message = "ClearCanvas";
                connection.invoke("SendClear", user, message).catch(function (err) {
                    return console.error(err.toString());
                });
            });
        });

    });
});
