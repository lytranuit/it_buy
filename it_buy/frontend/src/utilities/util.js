import moment from "moment/moment";
const formatSize = (bytes) => {
  if (bytes === 0) {
    return "0 B";
  }

  let k = 1000,
    dm = 3,
    sizes = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"],
    i = Math.floor(Math.log(bytes) / Math.log(k));

  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i];
};
const formatTime = (sec) => {
  let formatted = moment.utc(sec * 1000).format("HH:mm:ss");
  if (sec < 3600) {
    formatted = moment.utc(sec * 1000).format("mm:ss");
  }
  return formatted;
};
const formatPrice = (value, toFixed = 2) => {

  let val = Math.round(value * Math.pow(10, toFixed)) / Math.pow(10, toFixed);
  val = val.toFixed(toFixed).replace(",", ".");
  val = val.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
  return val.toString().replace(/\.0+$/, ''); // remove trailing zeros
};
const formatDate = (value, fomat = "YYYY-MM-DD") => {
  return moment(value).format(fomat);
};

const printTrigger = (link) => {
  $("body").append(
    '<iframe class="iFramePdf" src="' +
    link +
    '" style="display:none;"></iframe>'
  );
  var getMyFrame = $(".iFramePdf").last()[0];
  console.log(getMyFrame);
  getMyFrame.focus();
  getMyFrame.contentWindow.print();
};
const download = (name) => {
  if (!name) return name;
  if (
    name.toLowerCase().indexOf(".pdf") != -1 ||
    name.toLowerCase().indexOf(".png") != -1 ||
    name.toLowerCase().indexOf(".jpg") != -1 ||
    name.toLowerCase().indexOf(".jpeg") != -1 ||
    name.toLowerCase().indexOf(".gif") != -1 ||
    name.toLowerCase().indexOf(".tiff") != -1
  ) {
    return null;
  }
  return name;
};
const isMobile = () => {
  // console.log(window.innerWidth)
  if (window.innerWidth <= 760) {
    return true;
  }
  else {
    return false;
  }
}
const getZoom = () => {
  let fixWidth = 1745;
  let zoom = isMobile() ? 1 : window.innerWidth / fixWidth;
  if (zoom > 0.9) {
    zoom = 1;
  }
  return zoom;
}
export {
  formatSize,
  formatTime,
  formatPrice,
  formatDate,
  printTrigger,
  download,
  isMobile,
  getZoom
};
