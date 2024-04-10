import repository from "../service/repository";

const resoure = "dutru";
export default {
  save(params) {
    return repository
      .post(`/v1/${resoure}/Save`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  savedoima(params) {
    return repository
      .post(`/v1/${resoure}/savedoima`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  thongbaodoima(params) {
    return repository
      .post(`/v1/${resoure}/thongbaodoima`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  saveDinhkem(params) {
    return repository
      .post(`/v1/${resoure}/saveDinhkem`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  savenhanhang(params) {
    return repository
      .post(`/v1/${resoure}/savenhanhang`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  xoachitietdinhkem(params) {
    return repository
      .post(`/v1/${resoure}/xoachitietdinhkem`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  xuatpdf(id, is_view = false) {
    return repository
      .post(
        `/v1/${resoure}/xuatpdf`,
        { id: id, is_view: is_view },
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      )
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
  getFiles(id) {
    return repository
      .get(`/v1/${resoure}/getFiles`, { params: { id: id } })
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
  getNhanhang(id) {
    return repository
      .get(`/v1/${resoure}/getNhanhang`, { params: { id: id } })
      .then((res) => res.data);
  },

  getMuahang(id) {
    return repository
      .get(`/v1/${resoure}/getMuahang`, { params: { id: id } })
      .then((res) => res.data);
  },
  addcomment(params) {
    return repository
      .post(`/v1/${resoure}/addcomment`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  morecomment(model_id, from_id) {
    return repository
      .get(`/v1/${resoure}/morecomment`, {
        params: { id: model_id, from_id: from_id },
      })
      .then((res) => res.data);
  },
};
