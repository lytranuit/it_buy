import repository from "../service/repository";

const resoure = "api";
export default {
  nhacc() {
    return repository.get(`/v1/${resoure}/nhacc`).then((res) => res.data);
  },
  materials() {
    return repository.get(`/v1/${resoure}/materials`).then((res) => res.data);
  },
  departments() {
    return repository.get(`/v1/${resoure}/departments`).then((res) => res.data);
  },
};
