import { ref, computed } from "vue";
import { defineStore } from "pinia";
import Api from "../api/Api";

export const useGeneral = defineStore("general", () => {
  const materials = ref([]);
  const VattuGroup = ref([]);
  const NVLGroup = ref([]);
  const supliers = ref([]);
  const producers = ref([]);
  const materialGroup = ref([]);
  const khuvuc = ref([]);
  const products = ref([]);
  const kho = ref([]);
  const nhom = ref([]);
  const PLHH = ref([]);

  const is_get_tonkhoNVL = ref();
  const is_get_tonkhoVattu = ref();
  const is_get_nhom = ref();
  const is_get_kho = ref();
  const is_get_materials = ref();
  const is_get_supliers = ref();
  const is_get_producers = ref();
  const is_get_materialGroup = ref();
  const is_get_khuvuc = ref();
  const is_get_products = ref();
  const is_get_PLHH = ref();

  const fetchTonkhoVattu = (cache = true) => {
    if (cache == true && is_get_tonkhoVattu.value) return VattuGroup.value;
    is_get_tonkhoVattu.value = true;
    return Api.tonkhoVattu().then((response) => {
      VattuGroup.value = response;
      return response;
    });
  };

  const fetchTonkhoNVL = (cache = true) => {
    if (cache == true && is_get_tonkhoNVL.value) return NVLGroup.value;
    is_get_tonkhoNVL.value = true;
    return Api.tonkhoNVL().then((response) => {
      NVLGroup.value = response;
      return response;
    });
  };

  const fetchNhom = (cache = true) => {
    if (cache == true && is_get_nhom.value) return nhom.value;
    is_get_nhom.value = true;
    return Api.nhom().then((response) => {
      nhom.value = response;
      return response;
    });
  };

  const fetchMaterials = (cache = true) => {
    if (cache == true && is_get_materials.value) return materials.value;
    is_get_materials.value = true;
    return Api.materials().then((response) => {
      materials.value = response;
      return response;
    });
  };

  const fetchProducts = (cache = true) => {
    if (cache == true && is_get_products.value) return products.value;
    is_get_products.value = true;
    return Api.products().then((response) => {
      products.value = response;
      return response;
    });
  };
  const fetchKhuvuc = (cache = true) => {
    if (cache == true && is_get_khuvuc.value) return khuvuc.value;
    is_get_khuvuc.value = true;
    return Api.khuvuc().then((response) => {
      khuvuc.value = response;
      return response;
    });
  };
  const fetchPLHH = (cache = true) => {
    if (cache == true && is_get_PLHH.value) return PLHH.value;
    is_get_PLHH.value = true;
    return Api.PLHH().then((response) => {
      PLHH.value = response;
      return response;
    });
  };
  const fetchKho = (cache = true) => {
    if (cache == true && is_get_kho.value) return kho.value;
    is_get_kho.value = true;
    return Api.kho().then((response) => {
      kho.value = response;
      return response;
    });
  };


  const changeMaterial = (row) => {
    // console.log(materials);
    var material = materials.value.find((item) => item.mahh == row.mahh);
    if (material) {
      var temp = Object.assign({}, material);
      delete temp.id;
      // console.log(temp);
      Object.assign(row, temp);
      for (var key in row) {
        if (
          key == "ids" ||
          key == "stt" ||
          key == "soluong" ||
          key == "mahh"
        ) {
        } else {
          row[key] = material[key];
        }
      }
    } else {
      for (var key in row) {
        if (key == "ids" || key == "stt") {
        } else {
          delete row[key];
        }
      }
    }
    changeProducer(row);
    // console.log(row);
  };
  const changeProducer = (row) => {
    if (row.mansx) {
      var nhasx = producers.value.find((item) => item.mansx == row.mansx);
      if (nhasx) {
        row.nhasx = nhasx.tennsx;
      } else {
        delete row.nhasx;
      }
    } else {
      delete row.nhasx;
    }
  };
  async function fetchNhacc(cache = true) {
    if (cache == true && is_get_supliers.value) return supliers.value;
    is_get_supliers.value = true;
    return Api.nhacc().then((response) => {
      supliers.value = response;
      return response;
    });
  }
  async function fetchNhasx() {
    if (is_get_producers.value) return producers.value;
    is_get_producers.value = true;
    return Api.nhasx().then((response) => {
      producers.value = response;
      return response;
    });
  }
  async function fetchMaterialGroup() {
    // console.log(materialGroup.value.length);
    if (is_get_materialGroup.value) return materialGroup.value;
    is_get_materialGroup.value = true;
    return Api.group_materials().then((response) => {
      materialGroup.value = response;
      return response;
    });
  }
  return {
    materialGroup,
    materials,
    supliers,
    producers,
    khuvuc,
    kho,
    products,
    nhom,
    PLHH,
    VattuGroup,
    NVLGroup,
    fetchMaterials,
    fetchMaterialGroup,
    fetchNhacc,
    fetchNhasx,
    changeMaterial,
    changeProducer,
    fetchKhuvuc,
    fetchKho,
    fetchProducts,
    fetchNhom,
    fetchPLHH,
    fetchTonkhoVattu,
    fetchTonkhoNVL,
  };
});
