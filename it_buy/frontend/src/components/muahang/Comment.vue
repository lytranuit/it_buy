<template>
  <div class="card">
    <div class="card-header">
      <b>Thảo luận</b>
    </div>
    <div class="card-body">
      <form id="binhluan" enctype="multipart/form-data">
        <input name="muahang_id" :value="model.id" type="hidden" />

        <InputGroup>
          <input type="file" class="d-none" name='file[]' multiple @change="changeFile" />
          <Textarea placeholder="Thêm bình luận ở đây" rows="1" required name="comment" />
          <Button label="" icon="pi pi-file" size="small" severity="success" @click="AddCommentFile"></Button>

          <Button label="Gửi" icon="pi pi-send" size="small" @click="add_comment"></Button>
        </InputGroup>
        <div v-html="uploadText" v-if="uploadText" class="mt-2"></div>
      </form>
      <hr />
      <ul class="list-unstyled" id="comment_box">
        <li class="media comment_box my-2" :data-read="comment.is_read" v-for="(comment, index) in comments">
          <img class="mr-3 rounded-circle" :src="comment.user.image_url" width="50" alt="" />
          <div class="media-body border-bottom" style="display: grid">
            <h5 class="mt-0 mb-1" style="font-size: 14px;">
              {{ comment.user.fullName }}
              <small class="text-muted">
                -
                {{ formatDate(comment.created_at, "HH:mm DD/MM/YYYY") }}</small>
            </h5>
            <div class="mb-2" style="white-space: pre-wrap">
              {{ comment.comment }}
            </div>
            <div class="mb-2 attach_file file-box-content">
              <div class="file-box" v-for="(file, index1) in comment.files">
                <a :href="file.url" :download="file.name" class="download-icon-link">
                  <i class="dripicons-download file-download-icon"></i>
                </a>
                <div class="text-center">
                  <i class="far fa-file text-danger"></i>
                  <h6 class="text-truncate" :title="file.name">
                    {{ file.name }}
                  </h6>
                  <small class="text-muted">{{ file.ext }}</small>
                </div>
              </div>
            </div>
          </div>
        </li>
      </ul>
      <div class="text-center load_more" @click="getComments()">
        <button class="btn btn-primary btn-sm px-5">Xem thêm bình luận</button>
      </div>
    </div>
  </div>
</template>
<script setup>
import { onMounted, ref } from "vue";
import Api from "../../api/Api";
import { formatDate } from "../../utilities/util";
import { useMuahang } from "../../stores/muahang";
import { storeToRefs } from "pinia";
import muahangApi from "../../api/muahangApi";
import InputGroup from 'primevue/inputgroup';
import Textarea from 'primevue/textarea';
import Button from 'primevue/button';
const store_muahang = useMuahang();
const { model } = storeToRefs(store_muahang);
const comments = ref([]);
const uploadText = ref("");
const getComments = async () => {
  /// Lấy comments
  var model_id = model.value.id;
  var from_id;
  if (comments.value.length > 0) {
    from_id = comments.value[comments.value.length - 1].id;
  }
  var ress = await muahangApi.morecomment(model_id, from_id);
  var comments_tmp = ress.comments;
  if (comments_tmp.length == 10) {
    comments_tmp.pop();
  } else {
    $(".load_more").remove();
  }
  comments.value = comments.value.concat(comments_tmp);
}
const AddCommentFile = () => {
  $("[name='file[]']").click();
}
const changeFile = () => {
  uploadText.value = $("[name='file[]']")[0].files.length + " Files. Nhấn <i class='pi pi-send'></i> để gửi tin nhắn."
}
const add_comment = async (e) => {
  e.preventDefault();
  var comment = $("[name=comment]").val();
  var files = $("[name='file[]']")[0].files;
  //console.log(files);
  //return false;
  if (comment == "" && !files.length) {
    alert("Mời nhập bình luận!");
    return false;
  }
  var form = $("#binhluan")[0];
  var formData = new FormData(form);

  $("#binhluan").trigger("reset");
  uploadText.value = null;
  var result = await muahangApi.addcomment(formData);
  if (result.success) {
    var comment = result.comment;
    comments.value.unshift(comment);
  }
}
onMounted(() => {
  getComments();
})
</script>

<style lang="scss"></style>
