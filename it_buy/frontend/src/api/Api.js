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
    return repository.get(`/v1/${resoure}/group_materials`).then((res) => res.data);
  },
  departments() {
    return repository.get(`/v1/${resoure}/departments`).then((res) => res.data);
  },
  HomeBadge() {
    return repository.get(`/v1/${resoure}/HomeBadge`).then((res) => res.data);
  },
};
