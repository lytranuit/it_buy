import repository from "../service/repository";

const resoure = "muahang";
export default {
  xuatpdf(id) {
    return repository
      .post(
        `/v1/${resoure}/xuatpdf`,
        { id: id },
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      )
      .then((res) => res.data);
  },
  xoadinhkem(params) {
    return repository
      .post(`/v1/${resoure}/xoadinhkem`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  save(params) {
    return repository
      .post(`/v1/${resoure}/Save`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  saveLinkChitiet(params) {
    return repository
      .post(`/v1/${resoure}/saveLinkChitiet`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  baogia(id) {
    return repository
      .post(
        `/v1/${resoure}/baogia`,
        { id: id },
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      )
      .then((res) => res.data);
  },

  saveNcc(params) {
    return repository
      .post(`/v1/${resoure}/saveNcc`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  delete(id) {
    return repository
      .post(
        `/v1/${resoure}/Delete`,
        { id: id },
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      )
      .then((res) => res.data);
  },
  table(params) {
    return repository
      .post(`/v1/${resoure}/Table`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  tableChitiet(params) {
    return repository
      .post(`/v1/${resoure}/TableChitiet`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  get(id) {
    return repository
      .get(`/v1/${resoure}/Get`, { params: { id: id } })
      .then((res) => res.data);
  },
};
