import repository from "../service/repository";

const resoure = "danhgianhacungcap";
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
  thongbao(params) {
    return repository
      .post(`/v1/${resoure}/thongbao`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  chapnhan(id) {
    return repository
      .post(
        `/v1/${resoure}/chapnhan`,
        { id: id },
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      )
      .then((res) => res.data);
  },
  chapnhanDanhgia(params) {
    return repository
      .post(
        `/v1/${resoure}/chapnhanDanhgia`,
        params,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      )
      .then((res) => res.data);
  },
  saveDanhgia(params) {
    return repository
      .post(
        `/v1/${resoure}/saveDanhgia`,
        params,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      )
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
  get(id) {
    return repository
      .get(`/v1/${resoure}/Get`, { params: { id: id } })
      .then((res) => res.data);
  },
  getDanhgia(id) {
    return repository
      .get(`/v1/${resoure}/getDanhgia`, { params: { id: id } })
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
