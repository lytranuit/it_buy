import repository from "../service/repository";

const resoure = "muahang";
export default {
  xuatpdf(id, is_view = false, loaimau = 0) {
    return repository
      .post(
        `/v1/${resoure}/xuatpdf`,
        { id: id, is_view: is_view, loaimau: loaimau },
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
  xoancc(params) {
    return repository
      .post(`/v1/${resoure}/xoancc`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  xoadinhkemncc(params) {
    return repository
      .post(`/v1/${resoure}/xoadinhkemncc`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },

  xuatexcel(params) {
    return repository
      .post(`/v1/${resoure}/xuatexcel`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  xuatexcelchitiet(params) {
    return repository
      .post(`/v1/${resoure}/xuatexcelchitiet`, params, {
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
  saveUynhiemchi(params) {
    return repository
      .post(`/v1/${resoure}/saveUynhiemchi`, params, {
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
  saveChitiet(params) {
    return repository
      .post(`/v1/${resoure}/saveChitiet`, params, {
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
  saveHangmau(params) {
    return repository
      .post(`/v1/${resoure}/saveHangmau`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  saveQuidoi(chonmua_id, quidoi) {
    return repository
      .post(
        `/v1/${resoure}/saveQuidoi`,
        { chonmua_id: chonmua_id, quidoi: quidoi },
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      )
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
  thongbaothanhtoan(params) {
    return repository
      .post(`/v1/${resoure}/thongbaothanhtoan`, params, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
      .then((res) => res.data);
  },
  xuatdondathang(id) {
    return repository
      .post(
        `/v1/${resoure}/xuatdondathang`,
        { id: id },
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      )
      .then((res) => res.data);
  },
  savethanhtoan(params) {
    return repository
      .post(`/v1/${resoure}/savethanhtoan`, params, {
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
  getHistory(mahh) {
    return repository
      .get(`/v1/${resoure}/getHistory`, { params: { mahh: mahh } })
      .then((res) => res.data);
  },
  get(id) {
    return repository
      .get(`/v1/${resoure}/get`, { params: { id: id } })
      .then((res) => res.data);
  },
  Getnccs(id) {
    return repository
      .get(`/v1/${resoure}/Getnccs`, { params: { id: id } })
      .then((res) => res.data);
  },
  getDonhang(id) {
    return repository
      .get(`/v1/${resoure}/getDonhang`, { params: { id: id } })
      .then((res) => res.data);
  },

  getNhanhang(id) {
    return repository
      .get(`/v1/${resoure}/getNhanhang`, { params: { id: id } })
      .then((res) => res.data);
  },
  getUserNhanhang(id) {
    return repository
      .get(`/v1/${resoure}/getUserNhanhang`, { params: { muahang_id: id } })
      .then((res) => res.data);
  },
  QrNhanhang(id) {
    return repository
      .get(`/v1/${resoure}/QrNhanhang`, { params: { muahang_id: id } })
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
  getFiles(id) {
    return repository
      .get(`/v1/${resoure}/getFiles`, { params: { id: id } })
      .then((res) => res.data);
  },
};
