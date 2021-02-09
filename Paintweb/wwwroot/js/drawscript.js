function clearCanvas() {
    const canvas = $("#MainCanvas")[0];
    const context = canvas.getContext('2d');
    context.clearRect(0, 0, canvas.width, canvas.height);
}

$(function () {
    var jQueryCanvas = $("#MainCanvas");
    CanvasManager.prototype = new Object(jQueryCanvas.__proto__);
    let canvasManager = new CanvasManager(jQueryCanvas);
    let hub = new Hub(canvasManager);
    canvasManager.hub = hub;
    window.onbeforeunload = function () {
        hub.disconnect();
    };
    document.getElementById("ConnectToGroupButton").addEventListener("click", () => { hub.init(); });
    document.getElementById("ClearCanvasButton").addEventListener("click", () => hub.RequestClearCanvas());
});
