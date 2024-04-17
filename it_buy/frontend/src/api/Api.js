import repository from "../service/repository";

const resoure = "api";
export default {
  nhasx() {
    return repository.get(`/v1/${resoure}/nhasx`).then((res) => res.data);
  },
  nhacc() {
    return repository.get(`/v1/${resoure}/nhacc`).then((res) => res.data);
  },
  materials() {
    return repository.get(`/v1/${resoure}/materials`).then((res) => res.data);
  },
  group_materials() {
    return repository
      .get(`/v1/${resoure}/group_materials`)
      .then((res) => res.data);
  },
  departments() {
    return repository.get(`/v1/${resoure}/departments`).then((res) => res.data);
  },
  departmentsofuser() {
    return repository
      .get(`/v1/${resoure}/departmentsofuser`)
      .then((res) => res.data);
  },
  HomeBadge() {
    return repository.get(`/v1/${resoure}/HomeBadge`).then((res) => res.data);
  },
  chiphi(tungay, denngay, timetype) {
    return repository
      .post(
        `/v1/${resoure}/chiphi`,
        {
          tungay: tungay,
          denngay: denngay,
          timetype: timetype,
        },
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      )
      .then((res) => res.data);
  },
  chiphibophan(tungay, denngay) {
    return repository
      .post(
        `/v1/${resoure}/chiphibophan`,
        {
          tungay: tungay,
          denngay: denngay,
        },
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      )
      .then((res) => res.data);
  },
  tabledutru(params) {
    return repository
      .post(`/v1/${resoure}/tabledutru`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  tablemuahang(params) {
    return repository
      .post(`/v1/${resoure}/tablemuahang`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  comments(params) {
    return repository
      .post(`/v1/${resoure}/comments`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
};
