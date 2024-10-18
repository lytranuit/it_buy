import { ref, computed } from "vue";
import { defineStore } from "pinia";
import Api from "../api/Api";

export const useGeneral = defineStore("general", () => {
  const materials = ref([]);
  const supliers = ref([]);
  const producers = ref([]);
  const materialGroup = ref([]);

  const fetchMaterials = (cache = true) => {
    if (cache == true && materials.value.length) return materials.value;
    return Api.materials().then((response) => {
      materials.value = response;
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
    if (cache == true && supliers.value.length) return supliers.value;
    return Api.nhacc().then((response) => {
      supliers.value = response;
      return response;
    });
  }
  async function fetchNhasx() {
    if (producers.value.length) return producers.value;
    return Api.nhasx().then((response) => {
      producers.value = response;
      return response;
    });
  }
  async function fetchMaterialGroup() {
    // console.log(materialGroup.value.length);
    if (materialGroup.value.length) return materialGroup.value;
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
    fetchMaterials,
    fetchMaterialGroup,
    fetchNhacc,
    fetchNhasx,
    changeMaterial,
    changeProducer,
  };
});
