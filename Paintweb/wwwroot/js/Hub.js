function Hub(canvasManager) {
    this.user = "user_1";
    this.group = "g1";
    this.connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
    this.canvasManager = canvasManager;
    this.isConnected = false;

    this.connection.on("ReceiveLine", (user, message) => {
        this.canvasManager.drawLine(JSON.parse(message));
    });

    this.connection.on("ClearCanvas", (user, message) => {
        console.log("Clear");
        clearCanvas();
    });

    this.connection.on("ReceiveText", (user, message) => {
        console.log(message);
    });

}

Hub.prototype.sendLine = function (line) {
    if (this.isConnected == true) {
        this.connection.invoke("SendLineToGroup", this.user, JSON.stringify(line)).catch(function (err) {
            return console.error(err.toString());
        });
    }
};

Hub.prototype.disconnect = function () {
    if (this.group != "")
        this.connection.invoke("RemoveFromGroup", this.user, this.group).catch(function (err) {
            alert(err.toString());
        });
}


Hub.prototype.JoinGroup = function (groupName) {
    this.group = groupName;
    this.connection.invoke("AddToGroup", this.user, groupName).catch(function (err) {
        return console.error(err.toString());
    });
}

Hub.prototype.SynchronizeCanvas = function () {
    this.connection.invoke("SynchronizeCanvas", this.user, "synchronizecanvas").catch(function (err) {
        return console.error(err.toString());
    });
}

Hub.prototype.RequestClearCanvas = function () {
    var message = "ClearCanvas";
    this.connection.invoke("SendClear", this.user, message).catch(function (err) {
        return console.error(err.toString());
    });
}

Hub.prototype.init = function () {
    this.connection.start().then( () => {
        this.isConnected = true;
        groupName = document.getElementById("GroupTextInput").value;
        this.JoinGroup(groupName);
        this.SynchronizeCanvas();
    });
}
