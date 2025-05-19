import repository from "../service/repository";

const resoure = "material";
export default {
  get(id) {
    return repository.get(`/v1/${resoure}/get/${id}`).then((res) => res.data);
  },
  create(formData) {
    return repository
      .post(`/v1/${resoure}/create`, formData)
      .then((res) => res.data);
  },
  edit(formData) {
    return repository
      .post(`/v1/${resoure}/edit`, formData)
      .then((res) => res.data);
  },
  uploadImage(params) {
    return repository
      .post(`/v1/${resoure}/uploadImage`, params, {
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
  save(params) {
    return repository
      .post(`/v1/${resoure}/save`, params, {
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
  xoadinhkem(params) {
    return repository
      .post(`/v1/${resoure}/xoadinhkem`, params, {
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
  getFiles(id) {
    return repository
      .get(`/v1/${resoure}/getFiles`, { params: { id: id } })
      .then((res) => res.data);
  },
};
