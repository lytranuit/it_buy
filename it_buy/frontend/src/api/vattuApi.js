import repository from "../service/repository";

const resoure = "vattu";
export default {
  table(params) {
    return repository
      .post(`/v1/${resoure}/Table`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  excel(params) {
    return repository
      .post(`/v1/${resoure}/excel`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  getTonkho(params) {
    return repository
      .get(`/v1/${resoure}/getTonkho`, { params: params })
      .then((res) => res.data);
  },
  tableNhap(params) {
    return repository
      .post(`/v1/${resoure}/tableNhap`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  tableNhapChitiet(params) {
    return repository
      .post(`/v1/${resoure}/tableNhapChitiet`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  saveNhap(params) {
    return repository
      .post(`/v1/${resoure}/saveNhap`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  removeNhap(params) {
    return repository
      .post(`/v1/${resoure}/removeNhap`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  getNhap(params) {
    return repository
      .get(`/v1/${resoure}/getNhap`, { params: params })
      .then((res) => res.data);
  },
  tableXuat(params) {
    return repository
      .post(`/v1/${resoure}/tableXuat`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  tableXuatChitiet(params) {
    return repository
      .post(`/v1/${resoure}/tableXuatChitiet`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  saveXuat(params) {
    return repository
      .post(`/v1/${resoure}/saveXuat`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  removeXuat(params) {
    return repository
      .post(`/v1/${resoure}/removeXuat`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  xacnhanNhap(params) {
    return repository
      .post(`/v1/${resoure}/xacnhanNhap`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  getXuat(params) {
    return repository
      .get(`/v1/${resoure}/getXuat`, { params: params })
      .then((res) => res.data);
  },
};
