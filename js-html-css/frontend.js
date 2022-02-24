const record = document.querySelector("#Record");
const start = document.querySelector("#Start");
const download = document.getElementsByClassName("Download");
const taskid = document.getElementById("taskid");
const tablo = document.getElementById("tablo");
const tablo2 = document.getElementById("tablo2");
let taskTemplate = {
  id: null,
  name: "",
  avarageFPS: "",
  processTime: "",
};

record.addEventListener("click", function () {
  alert("recording starts");
  const p = new Promise(function (resolve, reject) {
    broadcastUrl = $("#broadcastInput").val();
    $.ajax({
      crossDomain: true,
      url: "http://localhost:5973/api/task",
      type: "POST",
      dataType: "json",
      data: {
        url: broadcastUrl,
      },
      success: function (response) {
        taskid.value = response.TaskID;
        resolve(response.TaskID);
      },
      error: function (jqXHR, textStatus, errorThrown) {
        alert(textStatus, errorThrown);
        reject("Task Failed");
      },
    });
  })
    .then(function (processID) {
      $.ajax({
        crossDomain: true,
        url: "http://localhost:5973/api/task/" + processID + "/start",
        type: "POST",
        dataType: "json",
        data: {},
        success: function (response) {
          //alert("kayit islemi basladi");
          taskTemplate.id = processID;
          taskTemplate.name = "TestRecordingTask_" + processID + ".mxf";
          sessionStorage.setItem("start", JSON.stringify(taskTemplate));
        },
        error: function (jqXHR, textStatus, errorThrown) {
          alert(textStatus, errorThrown);
        },
      });
    })
    .catch(function (e) {
      alert(e);
    });
});

// download tarafi yapilacak.

let listArray = [];

$(document).ready(function () {
  $(document).on("click", "#Stop", function () {
    alert("recording stops");
    $.ajax({
      crossDomain: true,
      url: "http://localhost:5973/api/task/" + taskid.value + "/stop",
      type: "POST",
      dataType: "json",
      data: {},
      success: function (response) {
        const start = sessionStorage.getItem("start");
        const startObject = JSON.parse(start);
        const averageFpsJson = sessionStorage.getItem("averageFps");
        const averageFps = JSON.parse(averageFpsJson);
        const timeCodeJson = sessionStorage.getItem("timeCode");
        const timeCode = JSON.parse(timeCodeJson);
        startObject.averageFps = averageFps;
        startObject.timeCode = timeCode;
        listArray.push(startObject);
        sessionStorage.setItem("list", JSON.stringify(listArray));
        sessionStorage.setItem("start", "");
      },
      error: function (jqXHR, textStatus, errorThrown) {
        alert(textStatus, errorThrown);
      },
    });
  });
});

setInterval(function () {
  const start = sessionStorage.getItem("start");
  if (start) {
    const startObject = JSON.parse(start);

    let htm = "";

    $.ajax({
      crossDomain: true,
      url: "http://localhost:5973/api/task/" + startObject.id,
      type: "GET",
      dataType: "json",
      data: {},
      success: function (response) {
        startObject.timeCode = response.time_elapsed.timecode;
        startObject.averageFps = response.stats.average_fps;
        startObject.currentFps = response.stats.current_fps;

        sessionStorage.setItem(
          "averageFps",
          JSON.stringify(startObject.averageFps)
        );
        sessionStorage.setItem(
          "timeCode",
          JSON.stringify(startObject.timeCode)
        );

        htm +=
          "<tr> <th>Process ID</th> <th>Name</th> <th>Average FPS</th><th>Current FPS</th> <th>Process Time</th>  </tr> <tr> <td>" +
          startObject.id +
          "</td>  <td>" +
          startObject.name +
          "</td> <td>" +
          (startObject.averageFps ? startObject.averageFps.toFixed(3) : 0) +
          "</td> <td>" +
          (startObject.currentFps ? startObject.currentFps.toFixed(3) : 0) +
          "</td> <td>" +
          startObject.timeCode +
          "</td>  </tr>";
        tablo.innerHTML = htm;
      },
    });
  }

  if (sessionStorage.getItem("list")) {
    const listArray = sessionStorage.getItem("list");
    const list = JSON.parse(listArray);
    let htm2 = "";

    for (const start of list) {
      htm2 +=
        "<tr> <th>Process ID</th> <th>Name</th> <th>Average FPS</th><th>Process Time</th> <th>Download</th> </tr> <tr> <td>" +
        start.id +
        "</td>  <td>" +
        start.name +
        "</td> <td>" +
        (start.averageFps ? start.averageFps.toFixed(3) : 0) +
        "</td> <td>" +
        start.timeCode +
        '</td> <td><a class="btn btn-warning text-uppercase download" href="files/TestRecordingTask_' +
        start.id +
        '.mxf" download>Download</a></td> </tr>';
    }
    tablo2.innerHTML = htm2;
  }
}, 1000);
