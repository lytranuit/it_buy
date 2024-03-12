import { ref, computed } from "vue";
import { defineStore } from "pinia";
import Api from "../api/Api";

export const useGeneral = defineStore("general", () => {
  const materials = ref([]);
  const supliers = ref([]);
  const fetchMaterials = (cache = true) => {
    if (cache == true && materials.value.length) return;
    return Api.materials().then((response) => {
      materials.value = response;
      return response;
    });
  };

  const changeMaterial = (mahh, row) => {
    var material = materials.value.find((item) => "m-" + item.id == mahh);
    if (material) {
      var temp = Object.assign({}, material);
      delete temp.id;
      Object.assign(row, temp);
    } else {
      for (var key in row) {
        if (key == "ids" || key == "stt") {
        } else {
          delete row[key];
        }
      }
    }
    console.log(row);
  };
  async function fetchNhacc() {
    if (supliers.value.length) return;
    return Api.nhacc().then((response) => {
      supliers.value = response;
      return response;
    });
  }
  return {
    materials,
    supliers,
    fetchMaterials,
    fetchNhacc,
    changeMaterial,
  };
});
