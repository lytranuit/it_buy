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
};
