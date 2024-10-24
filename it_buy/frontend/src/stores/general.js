import { ref, computed } from "vue";
import { defineStore } from "pinia";
import Api from "../api/Api";

export const useGeneral = defineStore("general", () => {
  const materials = ref([]);
  const supliers = ref([]);
  const producers = ref([]);
  const materialGroup = ref([]);
  const khuvuc = ref([]);
  const products = ref([]);

  const is_get_materials = ref();
  const is_get_supliers = ref();
  const is_get_producers = ref();
  const is_get_materialGroup = ref();
  const is_get_khuvuc = ref();
  const is_get_products = ref();

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
    products,
    fetchMaterials,
    fetchMaterialGroup,
    fetchNhacc,
    fetchNhasx,
    changeMaterial,
    changeProducer,
    fetchKhuvuc,
    fetchProducts

  };
});
