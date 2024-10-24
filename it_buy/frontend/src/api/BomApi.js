import repository from "../service/repository";

const resoure = "bom";
export default {
  get(mahh, colo) {
    return repository
      .get(`/v1/${resoure}/Get`, { params: { mahh: mahh, colo: colo } })
      .then((res) => res.data);
  },
  save(params) {
    return repository
      .post(`/v1/${resoure}/save`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  saveThaythe(params) {
    return repository
      .post(`/v1/${resoure}/saveThaythe`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  table(params) {
    return repository
      .post(`/v1/${resoure}/table`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  edit(params) {
    return repository
      .post(`/v1/${resoure}/edit`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  remove(params) {
    return repository
      .post(`/v1/${resoure}/remove`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
};
