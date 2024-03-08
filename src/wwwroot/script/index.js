// Nothing to see here except hacks and jank

var shouldFetchNew = true;
var actions = 0;
const visionImageUrl = "/vision/image?threadId=";
const enhanceImageUrl = "/vision/image/enhance";
var shouldStop = false;
var threadId = generateUuidv4();
var guid = generateUuidv4();
const imageFormat = "image/png";
const actionsTreshold = 2000;
var canvas;
var context;
var boundings;
var antiForgeryToken;
var mouseX = 0;
var mouseY = 0;
var isDrawing = false;
var incrementActionTimerId = 0;
var idleTimerId = 0;
var enhanceProgress = 0;

async function fetchAntiForgeryTokenAsync() {
  try {
    const response = await fetch("/antiforgery/token", {
      method: "GET",
      credentials: "include",
    });

    if (response.ok) {
      const token = response.headers.get("requestverificationtoken");
      if (token) {
        console.log("Anti-forgery token retrieved:", token);
        return token;
      } else {
        throw new Error("Anti-forgery token was not provided by the server.");
      }
    } else {
      throw new Error(`Server responded with status: ${response.status}`);
    }
  } catch (error) {
    console.error("Error fetching anti-forgery token:", error);
  }
}

function generateColors() {
  const colors = [
    // Light gray
    "#FFFFFF",
    "#EEEEEE",
    "#DDDDDD",
    "#CCCCCC",
    "#AAAAAA",

    // Dark gray
    "#888888",
    "#666666",
    "#444444",
    "#222222",
    "#000000",

    // Red
    "#FF0000",
    "#FF6464",
    "#FF3939",
    "#C60000",
    "#9B0000",

    // Brown (Dark yellow)
    "#FF9500",
    "#FFBB5B",
    "#FFA82D",
    "#DF8300",
    "#AE6600",

    // Orange
    "#FFC611",
    "#FFDC6B",
    "#FFD13F",
    "#F6BB00",
    "#BD9000",

    // Yellow
    "#FFFF00",
    "#FFFF64",
    "#FFFF39",
    "#C6C600",
    "#9B9B00",

    // Lime
    "#D1F900",
    "#E0FA59",
    "#D8F92C",
    "#B5D800",
    "#8DA800",

    // Green
    "#00FF00",
    "#61FE61",
    "#31FF31",
    "#00FF00",
    "#00DA00",

    // Light blue
    "#8888FF",
    "#E8E8FF",
    "#B0B0FF",
    "#6363FF",
    "#3C3CFC",

    // Blue
    "#0000FF",
    "#4949FC",
    "#1F1FFF",
    "#0000C0",
    "#010195",

    // Indigo
    "#2C16C7",
    "#6B5DD3",
    "#4B39CB",
    "#1F0E9B",
    "#170A79",

    // Magenta
    "#CD0074",
    "#D955A0",
    "#D02F8A",
    "#9F005A",
    "#7D0046",
  ];

  let colorDiv = document.getElementById("colors-box");

  colors.forEach((color) => {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "color-button";
    button.style = "background-color:" + color;
    button.addEventListener("click", function () {
      context.strokeStyle = color;
      const brushPreview = document.getElementById("brush-preview");
      brushPreview.style.backgroundColor = color;
    });
    colorDiv.appendChild(button);
  });
}

function setMouseCoordinates(event) {
  mouseX = event.clientX - boundings.left;
  mouseY = event.clientY - boundings.top;
}

function generateUuidv4() {
  return "00000000-0000-4000-1000-000000000000".replace(/[01]/g, function (c) {
    const uuid = (Math.random() * 16) | 0,
      v = c == "0" ? uuid : (uuid & 0x3) | 0x8;
    return uuid.toString(16);
  });
}

function initBrushSizeThing() {
  const brushSizeInput = document.getElementById("brush-size");
  const brushPreview = document.getElementById("brush-preview");

  brushSizeInput.addEventListener("input", function () {
    const size = brushSizeInput.value;
    context.lineWidth = size;
    brushPreview.style.width = size + "px";
    brushPreview.style.height = size + "px";
  });

  brushSizeInput.dispatchEvent(new Event("input"));
}

function incrementActions() {
  actions++;
  if (actions > actionsTreshold) {
    // heh
    actions = actionsTreshold / 2;
    const audioPlayer = document.getElementById("audio-player");
    if (!audioPlayer.paused) {
      return;
    }
    actions = 0;

    console.log(">> Auto-posting");
    postImageAndPlay();

    // Clear timed increment
    idleTimerId = setTimeout(() => {
      clearInterval(incrementActionTimerId);
      incrementActionTimerId = 0;
      clearInterval(idleTimerId);
      idleTimerId = 0;
    }, 5000);
  }
}

function initCanvas() {
  canvas = document.getElementById("paint-canvas");
  canvas.style.width = "100%";
  canvas.style.height = "100%";

  canvas.height = canvas.offsetHeight;
  canvas.width = canvas.offsetWidth;
  context = canvas.getContext("2d");
  context.fillStyle = "white";
  context.fillRect(0, 0, canvas.width, canvas.height);
  context.strokeStyle = "black";
  context.lineWidth = 1;

  generateColors();
  initBrushSizeThing();
  boundings = canvas.getBoundingClientRect();

  // Mouse Down Event
  canvas.addEventListener("mousedown", function (event) {
    setMouseCoordinates(event);
    isDrawing = true;

    if (incrementActionTimerId === 0) {
      incrementActionTimerId = setInterval(() => {
        incrementActions();
      }, 1);
    }

    // Start Drawing
    context.beginPath();
    context.moveTo(mouseX, mouseY);
  });

  // Mouse Move Event
  canvas.addEventListener("mousemove", function (event) {
    setMouseCoordinates(event);

    if (isDrawing) {
      context.lineTo(mouseX, mouseY);
      context.stroke();
      if (idleTimerId !== 0) {
        clearTimeout(idleTimerId);
      }
      incrementActions();
    }
  });

  // Mouse Up Event
  canvas.addEventListener("mouseup", function (event) {
    setMouseCoordinates(event);
    console.log("actions", actions);
    isDrawing = false;
  });

  canvas.addEventListener("mouseleave", function (event) {
    isDrawing = false;
  });

  const clearButton = document.getElementById("clear-butt");
  clearButton.addEventListener("click", function () {
    context.fillStyle = "white";
    context.fillRect(0, 0, canvas.width, canvas.height);
    actions = 0;
  });

  const saveButton = document.getElementById("save-butt");
  saveButton.addEventListener("click", function () {
    let imageName = prompt("Please enter image name");
    if (imageName === null) return;

    const canvas = document.getElementById("paint-canvas");
    const canvasDataURL = resizeCanvasR(canvas, 0.5).toDataURL("image/png");
    const a = document.createElement("a");
    a.href = canvasDataURL;
    a.download = imageName || "drawing";
    a.click();
  });

  const enhanceButton = document.getElementById("enhance-butt");
  enhanceButton.addEventListener("click", postImageAndEnhance);
}

function resizeCanvasR(canvas, ratio) {
  return resizeCanvasWH(canvas, canvas.width * ratio, canvas.height * ratio);
}

function resizeCanvasWH(canvas, width, height) {
  const resizedCanvas = document.createElement("canvas");
  const resizedContext = resizedCanvas.getContext("2d");

  resizedCanvas.width = width;
  resizedCanvas.height = height;

  resizedContext.drawImage(canvas, 0, 0, width, height);
  return resizedCanvas;
}

async function postImageAndEnhance(e) {
  if (e.target.disabled) return;
  e.target.disabled = true;
  enhanceProgress = 0;
  progress();
  const canvas = document.getElementById("paint-canvas");
  resizeCanvasR(canvas, 0.25).toBlob(async (blob) => {
    const data = new FormData();
    data.append("userImage", blob, "userImage.png");
    const response = await fetch(enhanceImageUrl, {
      method: "POST",
      // RequestVerificationToken: antiForgeryToken,
      body: data,
    });

    e.target.disabled = false;
    enhanceProgress = 100;

    if (!response.ok) throw new Error("Network response was not ok", response);

    const location = response.headers.get("Location");

    if (!location) throw new Error("Did not receive 'Location' header value.");

    const enhancedImages = document.getElementById("enhanced-images");
    const imgDiv = document.createElement("div");
    imgDiv.style.margin = "auto";
    const img = document.createElement("img");
    img.src = location;
    imgDiv.append(img);
    enhancedImages.prepend(imgDiv);
  }, "image/png");
}

async function postImageAndPlay() {
  const canvas = document.getElementById("paint-canvas");
  resizeCanvasR(canvas, 0.5).toBlob(async (blob) => {
    const data = new FormData();
    data.append("userImage", blob, "userImage.png");
    const response = await fetch(visionImageUrl + threadId + "&guid=" + guid, {
      method: "POST",
      // RequestVerificationToken: antiForgeryToken,
      body: data,
    });

    if (!response.ok) {
      throw new Error("Network response was not ok", response);
    }

    const location = response.headers.get("Location");

    if (!location) {
      throw new Error("Did not receive 'Location' header value.");
    }
    const audioPlayer = document.getElementById("audio-player");
    audioPlayer.src = location;
    audioPlayer.play();
  }, "image/png");
}

function progress() {
  enhanceProgress = 0;
  const outer = document.getElementById("img-progress-outer");
  // Hack in order to skip animation when resetting progress.
  const old = document.getElementById("img-progress");
  if (outer.contains(old)) {
    old.remove();
  }
  const elem = document.createElement("div");
  elem.id = "img-progress";
  outer.append(elem);
  const id = setInterval(frame, 200);
  function frame() {
    if (enhanceProgress >= 98) {
      clearInterval(id);
      enhanceProgress = 100;
    } else {
      enhanceProgress = enhanceProgress + (100 - enhanceProgress) / 75;
    }
    elem.style.width = enhanceProgress + "%";
  }
}

document.addEventListener("DOMContentLoaded", function () {
  initCanvas();
});
